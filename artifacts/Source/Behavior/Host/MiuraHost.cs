using AstmDriver.Abstractions;
using AstmDriver.Commons;
using System.Threading;

namespace AstmDriver.Behavior.Host
{
    /// <summary>
    /// Хост, реализующий специфическое подключение к Miura
    /// на базе Astm 
    /// </summary>
    public class MiuraHost : AstmHost
    {

        public MiuraHost(ITransportChannel channel, ILogger logger) : base(channel, logger)
        {
        }

        internal override void OnConnectiongHandler()
        {
            //TransportChannel.WriteData($"{KeepAlive}");
            //Thread.Sleep(250);
            Status = ChannelStatus.Receiving;
        }
    }
}
