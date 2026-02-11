using System.Diagnostics;
using LIM.Astm.Models;

namespace LIM.Astm.Abstractions;

public interface ITransport
{
    event EventHandler<ChannelDataEventArgs> ChannelDataReceived;
    event EventHandler<ChannelDataEventArgs> ChannelDataTransmitted;
}