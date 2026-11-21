using AstmDriver.Abstractions;
using AstmDriver.Commons;
using AstmDriver.Core;
using AstmDriver.Exceptions;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace AstmDriver.Behavior.Host
{
    /// <summary>
    /// MLLP хост для работы с анализаторами реализующие поддержку HL7
    /// </summary>
    public class MllpHost : IHost
{
    private const char VT = '\u000B'; // 0x0B — начало MLLP
    private const char FS = '\u001C'; // 0x1C — конец MLLP
    private const char CR = '\r';     // 0x0D — завершающий CR

    private readonly ILogger _logger;
    private readonly StringBuilder _rxBuffer = new StringBuilder();
    private string _ipClient;

    protected internal bool _isDisposed;

    private ITransportChannel _transportChannel;
    public ITransportChannel TransportChannel
    {
        get => _transportChannel;
        set
        {
            if (_transportChannel != null && _transportChannel != value)
            {
                _transportChannel.OnReceiveData -= ChannelDataReceived;
                _transportChannel.OnConnecting -= TransportChannel_OnConnecting;
            }
            _transportChannel = value;
            _transportChannel.OnReceiveData += ChannelDataReceived;
            _transportChannel.OnConnecting += TransportChannel_OnConnecting;
        }
    }

    public ChannelStatus Status { get; set; }

    #region Events
    public event EventHandler<ChannelDataEventArgs> OnReceiveString;
    public event EventHandler OnConnectionClosed;
    public event EventHandler OnReceiveTimeOut;
    public event EventHandler OnEOTReceived;      // в HL7 не используется
    public event EventHandler<string> OnError;
    public event EventHandler<string> OnReconnect;
    #endregion

    public MllpHost(ITransportChannel channel, ILogger logger) : this(channel, 30,logger) { }

    public MllpHost(ITransportChannel channel, int timeOut, ILogger logger)
    {
        _logger = logger;
        TransportChannel = channel;
    }

    public string GetClientIP() => _ipClient;

    private void TransportChannel_OnConnecting(object sender, string e)
    {
        _ipClient = e;
    }

    #region Receive — MLLP framing
    private void ChannelDataReceived(object sender, ChannelDataEventArgs e)
    {
        string buffer = e.Data;
        if (string.IsNullOrEmpty(buffer)) return;

        foreach (char ch in buffer)
        {
            switch (ch)
            {
                case VT:
                    // начало нового MLLP-фрейма — сбрасываем накопленное
                    _rxBuffer.Clear();
                    break;

                case FS:
                    {
                        string hl7 = _rxBuffer.ToString();
                        _rxBuffer.Clear();
                        if (hl7.Length > 0)
                        {
                            _logger.Debug("HL7 IN: " + hl7);
                            OnReceiveString?.Invoke(this,
                                new ChannelDataEventArgs(hl7, e.RawData));
                        }
                    }
                    break;
                default:
                    _rxBuffer.Append(ch);
                    break;
            }
        }
    }
    #endregion

    #region Send — MLLP wrapping
    public bool EstablishSendMode()
    {
        // В MLLP нет handshake — просто выставляем флаг
        Status = ChannelStatus.Sending;
        return true;
    }

    public void StopSendMode()
    {
        Status = ChannelStatus.Idle;
    }

    public void SendMessage(string hl7Message)
    {
        if (string.IsNullOrEmpty(hl7Message)) return;

        // Защита: если вызывающий случайно передал уже обёрнутое сообщение —
        // не оборачиваем повторно
        if (hl7Message[0] == VT) 
        {
            TransportChannel.WriteData(hl7Message);
            return;
        }

        var framed = new StringBuilder(hl7Message.Length + 3);
        framed.Append(VT);
        framed.Append(hl7Message);
        framed.Append(FS);
        framed.Append(CR);

        TransportChannel.WriteData(framed.ToString());
    }
    #endregion

    public void Connect()
    {
        try
        {
            _logger.Info("Connecting To Host...");
            TransportChannel.Connect();
            _logger.Info("Connected");
        }
        catch (SocketException sxc)
        {
            var message = $"Ошибка при открытии соединения (SocketError={sxc.ErrorCode})";
            _logger.Error(message);
            OnError?.Invoke(this, message);
            throw new HostConnectionException(message, sxc);
        }
        catch (Exception ex)
        {
            var message = "Ошибка при открытии соединения";
            _logger.Error($"Error Opening Connection\n{ex.Message}");
            OnError?.Invoke(this, message);
            throw new HostConnectionException(message, ex);
        }
    }

    public void DisConnect()
    {
        try
        {
            _logger.Info("Disconnecting...");
            TransportChannel.DisConnect();
            Status = ChannelStatus.Idle;
            OnConnectionClosed?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            _logger.Error($"Error Closing Connection\n{ex.Message}");
            throw new HostConnectionException("Error closing Connection.", ex);
        }
    }

    public void StartReceiveTimeoutTimer() { /* в HL7 не нужен */ }

    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed) return;
        (TransportChannel as IDisposable)?.Dispose();
        _isDisposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
}