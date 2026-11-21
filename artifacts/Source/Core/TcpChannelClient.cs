using AstmDriver.Commons;
using AstmDriver.Exceptions;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using AstmDriver.Abstractions;

namespace AstmDriver.Core
{
public class TcpChannelClient : AbstractTcpChannel
{
    private TcpClient _tcpClient;
    private volatile bool _stopRequested;

    private const int ReceiveBufferSize = 8192;
    private const int ReconnectBaseDelayMs = 1000;   // 1s
    private const int ReconnectMaxDelayMs  = 30000;  // 30s

    public TcpChannelClient(string hostAddress, ushort port, string encoding)
        : base(hostAddress, port, encoding, LoggerFactory.GetLogger(typeof(TcpChannelClient)))
    {
        ValidateEncoding();
    }

    private void ValidateEncoding()
    {
        // MLLP требует, чтобы байты 0x0B / 0x1C не встречались внутри символов.
        if (_encoding is System.Text.UnicodeEncoding)
        {
            throw new TransportException(
                "HL7/MLLP: UTF-16 (Unicode) несовместим — байты 0x0B/0x1C " +
                "могут встретиться внутри символов. Используйте ASCII, UTF-8 без BOM " +
                "или 1251.");
        }

        if (_encoding is System.Text.UTF8Encoding u8 && u8.GetPreamble().Length > 0)
        {
            throw new TransportException(
                "HL7/MLLP: UTF-8 с BOM несовместим. Используйте UTF-8 без BOM.");
        }
    }

    public override void Connect()
    {
        _stopRequested = false;
        int attempt = 0;

        while (!_stopRequested)
        {
            try
            {
                _logger.Info($"Connecting to {_hostAddress}:{_port}...");

                _tcpClient = new TcpClient(AddressFamily.InterNetwork);
                _tcpClient.Connect(_hostAddress, _port);

                _socket = _tcpClient.Client;
                _isConnected = true;
                attempt = 0;  
                _diag_timer.Start();

                var remoteIp = ((System.Net.IPEndPoint)_socket.RemoteEndPoint)
                                   .Address.ToString();
                OnConnectingInvoke(remoteIp);

                _logger.Info("Connected");

                ReceiveLoop(); 
                
                _logger.Info("Receive loop terminated");
                OnDisconnectingInvoke("The server closed the connection");
            }
            catch (ObjectDisposedException)
            {
                _logger.Info("Connection closed by DisConnect()");
                break;
            }
            catch (SocketException sx)
            {
                _logger.Error($"SocketException: {sx.ErrorCode} — {sx.Message}");
                OnDisconnectingInvoke($"Connection lost: {sx.ErrorCode} {sx.Message}");
            }
            catch (Exception ex)
            {
                _logger.Error($"Connect error: {ex}");
                OnDisconnectingInvoke($"Connection error: {ex.Message}");
            }
            finally
            {
                _diag_timer.Stop();
                _isConnected = false;

                try { _tcpClient?.Close(); } catch { }
                try { _socket?.Dispose(); } catch { }
                _tcpClient = null;
                _socket = null;
            }

            if (_stopRequested) break;

            // Exponential backoff 1s, 2s, 4s, ... до 30s
            int delay = Math.Min(ReconnectBaseDelayMs * (1 << Math.Min(attempt, 5)),
                                 ReconnectMaxDelayMs);
            attempt++;
            _logger.Info($"Reconnecting in {delay} ms (attempt {attempt})");
            Thread.Sleep(delay);
        }

        _logger.Info("Connect loop exited");
    }

    private void ReceiveLoop()
    {
        byte[] buffer = new byte[ReceiveBufferSize];

        while (!_stopRequested && _isConnected)
        {
            int count;
            try
            {
                count = _socket.Receive(buffer);
            }
            catch (SocketException sx)
            {
                // 10054 — connection reset, 10053 — software caused abort,
                // 10057 — not connected, 10060 — timeout.
                _logger.Info($"Receive loop SocketException {sx.ErrorCode}: {sx.Message}");
                _isConnected = false;
                return;
            }
            catch (ObjectDisposedException)
            {
                // DisConnect() закрыл сокет
                _isConnected = false;
                return;
            }

            if (count == 0)
            {
                _logger.Info("Remote side closed the connection (Receive=0)");
                _isConnected = false;
                return;
            }

            string dataReceived = _encoding.GetString(buffer, 0, count);
            OnReceiveStringInvoke(new ChannelDataEventArgs(dataReceived));
        }
    }
    public new void DisConnect()
    {
        _stopRequested = true;
        base.DisConnect();
    }
}
}
