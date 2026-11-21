using AstmDriver.Commons;
using System;
using System.Threading.Tasks;

namespace AstmDriver.Abstractions
{
    public interface ITransportChannel
	{
		void ClearBuffers();
		void Connect();
		void DisConnect();
		void WriteData(string value);


		event EventHandler<ChannelDataEventArgs> OnReceiveData;
		event EventHandler<ChannelDataEventArgs> OnTransiveString;

		event EventHandler<string> OnConnecting;
		event EventHandler<string> OnDisconnecting;

		bool IsConnected { get; }
	}
}
