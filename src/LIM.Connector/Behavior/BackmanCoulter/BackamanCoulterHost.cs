using LIM.Connector.Abstraction;

namespace LIM.Connector.Behavior.BackmanCoulter;

public class BackmanCoulterHost : IHost
{
    private ITransportChannel _channel;
    private ChannelStatus _status;
    public ITransportChannel Channel => _channel;

    public ChannelStatus Status => _status;

    public BackmanCoulterHost(ITransportChannel channel)
    {
        if(channel == null)
            throw new ArgumentNullException(nameof(channel));
        _channel = channel;

        _status = ChannelStatus.Init;
    }

    public void Close()
    {
        throw new NotImplementedException();
    }

    public void Open()
    {
        throw new NotImplementedException();
    }

    public void SendMessage(string message)
    {
        throw new NotImplementedException();
    }
}