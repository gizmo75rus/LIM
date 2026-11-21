using System;

namespace LIM.Connector.Events;

public class ChannelDataEventArgs : EventArgs
{
    public string Data { get; set; }
    
    public string RawData { get; set; }
    
    public ChannelDataEventArgs(string data, string rawData)
    {
        Data = data ?? throw new ArgumentNullException(nameof(data));
        RawData = rawData ?? throw new ArgumentNullException(nameof(rawData));
    }
}