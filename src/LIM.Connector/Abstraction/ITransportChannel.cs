using System;
using LIM.Connector.Events;

namespace LIM.Connector.Abstraction;

public interface ITransportChannel
{
    event EventHandler<ChannelDataEventArgs> DataReceived;
    event EventHandler<ChannelDataEventArgs> DataSent;

    event EventHandler<string> OnConnecting;
    event EventHandler<string> OnDisconnecting;
    
    void Connect();
    void Disconnect();
}