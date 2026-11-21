using AstmDriver.Commons;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using AstmDriver.Abstractions;

namespace AstmDriver.Core
{
    public class TcpChannelServerV3 : AbstractTcpChannel
    {
        private TcpListener _listener;
        private volatile bool _stopRequested;

        public TcpChannelServerV3(string hostAddress, ushort port, string encoding)
            : base(hostAddress, port, encoding,LoggerFactory.GetLogger(typeof(TcpChannelServerV3)))
        {
        }

        public override void Connect()
        {
            _stopRequested = false;
            _listener = new TcpListener(IPAddress.Any, _port);
            _listener.Start();

            while (!_stopRequested)
            {
                TcpClient client = null;
                try
                {
                    client = _listener.AcceptTcpClient();
                }
                catch (SocketException) when (_stopRequested)
                {
                    break;
                }
                catch (ObjectDisposedException)
                {
                    break;
                }

                HandleClient(client);
            }
        }

        private void HandleClient(TcpClient client)
        {
            _socket = client.Client;
            _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            _isConnected = true;
            _diag_timer.Start();
            OnConnectingInvoke(((IPEndPoint)_socket.RemoteEndPoint).Address.ToString());

            byte[] buffer = new byte[8192];
            try
            {
                while (!_stopRequested && _isConnected)
                {
                    int count;
                    try
                    {
                        count = _socket.Receive(buffer);
                    }
                    catch (SocketException)
                    {
                        break;
                    }
                    catch (ObjectDisposedException)
                    {
                        break;
                    }

                    if (count == 0) break; // клиент закрыл соединение

                    var message =  _encoding.GetString(buffer, 0, count);
                    OnReceiveStringInvoke(new ChannelDataEventArgs(message,message));
                }
            }
            finally
            {
                _diag_timer.Stop();
                _isConnected = false;
                try
                {
                    client.Close();
                }
                catch
                {
                }

                _socket = null;
            }
        }

        public override void DisConnect()
        {
            _stopRequested = true;
            try
            {
                _listener?.Stop();
            }
            catch (Exception ex)
            {
                _logger.Debug($"Listener stop: {ex.Message}");
            }
            base.DisConnect();
        }
    }
}