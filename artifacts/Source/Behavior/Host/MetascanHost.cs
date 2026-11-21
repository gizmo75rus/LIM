using AstmDriver.Abstractions;
using AstmDriver.Commons;
using System.Threading;

namespace AstmDriver.Behavior.Host
{
    /// <summary>
    /// Хост, реализующий специфическое подключение к Metascan
    /// на базе Astm 
    /// </summary>
    public class MetascanHost : AstmHost
    {
        public MetascanHost(ITransportChannel channel, ILogger logger) : base(channel, logger)
        {

        }
        

        internal override void OnConnectiongHandler() {
            TransportChannel.WriteData($"{ENQ}");
            Thread.Sleep(250);
            TransportChannel.WriteData($"{EOT}");
            Thread.Sleep(250);
            Status = ChannelStatus.Idle;
        }
    }
}
