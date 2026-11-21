using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace AstmDriver.Commons
{
    public static class StreamExtensions
	{
		public static bool ProbeConnection(this Socket socket)
        {
			var connection = IPGlobalProperties.GetIPGlobalProperties()
					  .GetActiveTcpConnections()
					  .FirstOrDefault(x => EndpointEquals(x.LocalEndPoint, socket.LocalEndPoint));

			bool connected = (connection != null ? connection.State : TcpState.Unknown) == TcpState.Established;
			if (connected)
			{
				var host = ((IPEndPoint)socket?.RemoteEndPoint).Address.ToString();
				return PingHost(host);
			}
			else
			{
				return false;
			}
		}

		public static bool IsConnected(this Socket socket)
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

		private static bool PingHost(string nameOrAddress)
		{
			bool value = false;
			Ping pinger = null;

			try
			{
				pinger = new Ping();
				
				PingReply reply = pinger.Send(nameOrAddress);
				value = reply.Status == IPStatus.Success;
			}
			catch (PingException)
			{
				// Discard PingExceptions and return false;
			}
			finally
			{
				if (pinger != null)
				{
					pinger.Dispose();
				}
			}

			return value;
		}

		private static readonly AddressFamily[] addressFamilies = 
		{
			AddressFamily.InterNetwork,
			AddressFamily.InterNetworkV6 
		};

		private static bool EndpointEquals(EndPoint target, EndPoint source)
		{
			if (target is IPEndPoint ipEndPoint1 && source is IPEndPoint ipEndPoint2)
			{
				if (ipEndPoint1.AddressFamily != ipEndPoint2.AddressFamily &&
					addressFamilies.Contains(ipEndPoint1.AddressFamily) &&
					addressFamilies.Contains(ipEndPoint2.AddressFamily))
				{
					var ipAddress1AsV6 = ipEndPoint1.Address.MapToIPv6();
					var ipAddress2AsV6 = ipEndPoint2.Address.MapToIPv6();

					return ipAddress1AsV6.Equals(ipAddress2AsV6)
						&& ipEndPoint1.Port.Equals(ipEndPoint2.Port);
				}
			}

			return object.Equals(target, source);
		}

	}
}
