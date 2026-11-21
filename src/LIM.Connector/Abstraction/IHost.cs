using LIM.Connector.Abstraction;

namespace LIM.Connector.Abstraction;

public interface IHost
{
    ITransportChannel Channel { get; }
    ChannelStatus Status { get; }
    
    void Open();
    void Close();
    void SendMessage(string message);
}