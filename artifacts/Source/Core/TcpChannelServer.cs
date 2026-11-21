using AstmDriver.Commons;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AstmDriver.Abstractions;

namespace AstmDriver.Core
{
    public class TcpChannelServer : AbstractTcpChannel
    {
        private TcpClient _client;
        private TcpListener _server;

        public TcpChannelServer(string hostAddress, ushort port, string encoding) 
            : base(hostAddress, port, encoding, LoggerFactory.GetLogger(typeof(TcpChannelServer)))
        {
            _server = new TcpListener(IPAddress.Any, port);
        }

        public override void Connect()
        {
            _server.Start();

            try
            {
                while (true)
                {
                    _client = _server.AcceptTcpClient();
                    _isConnected = true;
                    _socket = _client.Client;
                    _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

                    _diag_timer.Start();

                    OnConnectingInvoke(((IPEndPoint)_socket?.RemoteEndPoint).Address.ToString());

                    byte[] array = new byte[2048];

                    while (_socket != null && _isConnected)
                    {
                        int available = _socket.Available;
                        if (available > 0)
                        {
                            int count = _socket.Receive(array);
                            string dataReceived = _encoding.GetString(array, 0, count);
                            OnReceiveStringInvoke(new ChannelDataEventArgs(dataReceived));
                        }
                        else
                        {
                            Thread.Sleep(100);
                        }
                    }

                    OnDisconnectingInvoke("The client closed the connection");
                }

            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                OnDisconnectingInvoke("The client closed the connection");
               
            }
            finally
            {
                Thread.Sleep(1000);
                _diag_timer.Stop();
                _server.Stop();

                Connect();
            }
        }

    }
}
