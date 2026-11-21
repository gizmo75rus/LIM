using System;
using LIM.Connector.Events;

namespace LIM.Connector.Abstraction;

public interface ITransportChannel
{
    event EventHandler<ChannelDataEventArgs> DataReceived;
    event EventHandler<ChannelDataEventArgs> DataSent;
    
    void Connect();
    void Disconnect();
}