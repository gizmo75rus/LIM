using AstmDriver.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AstmDriver.Abstractions;

namespace AstmDriver.Core
{
    public class TcpChannelServerV2 : AbstractTcpChannel
    {
        public TcpChannelServerV2(string hostAddress, ushort port, string encoding) 
            : base(hostAddress, port, encoding,LoggerFactory.GetLogger(typeof(TcpChannelServerV2)))
        {
            _diag_timer.Enabled = false;
            _diag_timer.Stop();
        }

        public override void Connect()
        {
            var Listener = new TcpListener(IPAddress.Any, _port);
            Listener.Start();

            while (true)
            {

                TcpClient Client = null;
                try
                {
                    Client = Listener.AcceptTcpClient();
                }
                catch (SocketException se)
                {
                    Console.WriteLine("Ошибка на уровне сокета ===> " + se.Message);

                }
                catch (InvalidOperationException io)
                {
                    Console.WriteLine("Ошибка на уровне сокета ===> " + io.Message);
                }

                Thread thread = new Thread(new ParameterizedThreadStart(ClientThread));
                thread.Start(Client);
            }
        }

        private void ClientThread(object @params)
        {
            if (@params == null)
                return;

            var cl = (TcpClient)@params;
            _socket = cl.Client;

            OnConnectingInvoke(((IPEndPoint)_socket?.RemoteEndPoint).Address.ToString());
            try
            {
                while (IsSocketConnected(cl.Client))
                {
                    byte[] Buffer = new byte[1024];
                    int cnt;
                    while ((cnt = cl.GetStream().Read(Buffer, 0, Buffer.Length)) > 0)
                    {
                        var dataReceived = _encoding.GetString(Buffer, 0, cnt);
                        OnReceiveStringInvoke(new ChannelDataEventArgs(dataReceived));
                        Thread.Sleep(100);
                    };

                };

                OnDisconnectingInvoke("The client closed the connection");
                cl.GetStream().Close();
            }
            catch (Exception ex)
            {
                OnDisconnectingInvoke("The connection terminated");
            }
            finally
            {
                if (cl != null)
                    cl.Close();
            }
        }

        bool IsSocketConnected(Socket socket)
        {
            if (socket.Connected)
            {
                if ((socket.Poll(0, SelectMode.SelectWrite)) && (!socket.Poll(0, SelectMode.SelectError)))
                {
                    byte[] buffer = new byte[1];
                    if (socket.Receive(buffer, SocketFlags.Peek) == 0)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}
