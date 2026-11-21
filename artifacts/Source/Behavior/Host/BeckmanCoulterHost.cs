using AstmDriver.Abstractions;
using AstmDriver.Commons;
using AstmDriver.Core;
using AstmDriver.Exceptions;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace AstmDriver.Behavior.Host
{
    /// <summary>
    /// Хост реализующий подключение Beckman Coulter AU480 
    /// </summary>
    public class BeckmanCoulterHost : IHost
    {
        #region Private Fields
        private const char STX = '\u0002';
        private const char ETX = '\u0003';
        private const char ETB = '\u0017';
        private const char ENQ = '\u0005';
        private const char ACK = '\u0006';
        private const char NAK = '\u0015';
        private const char EOT = '\u0004';
        private const char NUL = '\0';
        private const char CR = '\r';
        private const char LF = '\n';

        private const int _maxFrameSize = 63993;
        private int _enqWaitTimeout = 10000;
        private int _ackWaitTimeout = 15000;

        private int _tryConnectAttems = 5;
        private int _currentAttemp = 0;
        private int _connectWaitTimeout = 15000;

        private readonly ILogger _logger;

        private ITransportChannel _transportChannel;
        private readonly StringBuilder _tempReceiveBuffer = new StringBuilder();
        private string _tempIntermediateFrameBuffer;

        private readonly EventWaitHandle _enqWaitObject = new EventWaitHandle(true, EventResetMode.ManualReset);
        private readonly EventWaitHandle _ackWaitObject = new EventWaitHandle(true, EventResetMode.ManualReset);
        private readonly EventWaitHandle _eotWaitObject = new EventWaitHandle(true, EventResetMode.ManualReset);
        private bool _ackReceived;
        private readonly System.Timers.Timer _receiveTimeOutTimer = new System.Timers.Timer();
        private int _frameNumber;
        private bool _lastFrameWasIntermediate;
        private string _ipClient;

        protected internal bool _isDisposed;

        #endregion

        #region Events
        public event EventHandler<ChannelDataEventArgs> OnReceiveString;
        public event EventHandler OnConnectionClosed;
        public event EventHandler OnReceiveTimeOut;
        public event EventHandler OnEOTReceived;
        public event EventHandler<string> OnError;
        public event EventHandler<string> OnReconnect;
        #endregion

        #region Constructors

        public BeckmanCoulterHost(ITransportChannel channel, ILogger logger) : this(channel, 30, logger)
        {
        }

        public BeckmanCoulterHost(ITransportChannel channel, int timeOut, ILogger logger)
        {
            TransportChannel = channel;
            _receiveTimeOutTimer.Interval = timeOut * 1000;
            _receiveTimeOutTimer.Enabled = false;
            _receiveTimeOutTimer.Elapsed += ReceiveTimeOutTimer_Elapsed;
            _logger = logger;
        }
        #endregion

        #region Public Properties
        private ChannelStatus _status;
        public ChannelStatus Status
        {
            get { return _status; }
            set { _status = value; }
        }

        public ITransportChannel TransportChannel
        {
            get => _transportChannel; set
            {
                if (_transportChannel != null && _transportChannel != value)
                {
                    _logger.Debug("Отписка от транспорт чаннел");
                    _transportChannel.OnReceiveData -= ChannelDataReceived;
                    _transportChannel.OnConnecting -= _transportChannel_OnConnecting;
                }
                _transportChannel = value;
                _transportChannel.OnReceiveData += ChannelDataReceived;
                _transportChannel.OnConnecting += _transportChannel_OnConnecting;
            }
        }

        public string GetClientIP() => _ipClient;

        private void _transportChannel_OnConnecting(object sender, string e)
        {
            _ipClient = e;
        }

        #endregion

        #region Private Methods
        private void ChannelDataReceived(object sender, ChannelDataEventArgs e)
        {
            string buffer = e.Data;
            if (buffer != null)
            {
                var clear = ControlCodesUtility.ClearControlCodes(buffer);
                OnReceiveString?.Invoke(this, new ChannelDataEventArgs(clear, buffer));
            }
        }

        private void ReceiveTimeOutTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Status = ChannelStatus.Idle;
            _receiveTimeOutTimer.Stop();
            OnReceiveTimeOut?.Invoke(this, new EventArgs());
        }


        #endregion

        public void Connect()
        {

            try
            {
                _logger.Info("\nConnecting To Host...");
                TransportChannel.Connect();
                _logger.Info("Connected");
            }
            catch (SocketException sxc)
            {
                if (sxc.ErrorCode == 10060)
                {
                    if (_currentAttemp >= _tryConnectAttems)
                    {
                        _currentAttemp = 0;
                        var date = DateTime.Now.AddMinutes(15);
                        OnError?.Invoke(this, $"Ну удалось установить подключение, следующая поптыка в {date.ToShortTimeString()}");
                        _logger.Info(sxc.Message);
                        Thread.Sleep(15 * 60 * 1000);
                        Connect();

                    }
                    else
                    {
                        _currentAttemp++;
                        OnReconnect?.Invoke(this, $"Подключение, попытка {_currentAttemp}/{_tryConnectAttems}");
                        Thread.Sleep(_connectWaitTimeout);
                        Connect();
                    }

                }
                else
                {
                    var message = "Ошибка при открытии соединения";
                    _logger.Error($"Error Opening Connection\n{sxc.Message}");
                    OnError?.Invoke(this, message);
                    throw new HostConnectionException(message, sxc);
                }
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
                _logger.Info("Disconnected");
                OnConnectionClosed?.Invoke(this, new EventArgs());
            }
            catch (Exception ex)
            {
                _logger.Error($"Error Closing Connection\n{ex.Message}");
                throw new HostConnectionException("Error closing Connection.", ex);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                IDisposable connection = TransportChannel as IDisposable;
                if (connection != null)
                {
                    connection.Dispose();
                }
                else
                {
                }
                _isDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public bool EstablishSendMode()
        {
            Status = ChannelStatus.Sending;
            return true;
        }

        public void SendMessage(string message)
        {

            TransportChannel.ClearBuffers();
            TransportChannel.WriteData(message);
        }

        public void StartReceiveTimeoutTimer()
        {
            
        }

        public void StopSendMode()
        {
            Status = ChannelStatus.Idle;
        }
    }
}
