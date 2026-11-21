using System;
using AstmDriver.Abstractions;
using AstmDriver.Commons;

namespace AstmDriver.Core
{
    public static class ChannelFactory
    {
        public static ITransportChannel CreateChannel(DriverConfig config)
        {
            if(config == null) 
                throw new ArgumentNullException(nameof(config));

            ITransportChannel channel = null;

            if (config.IsServerMode)
            {
                switch (config.ServerVersion)
                {
                    case 1:
                        channel = new TcpChannelServer(config.RemoteHost, config.Port, config.Encoding);
                        break;
                    default: case 2:
                        channel = new TcpChannelServerV2(config.RemoteHost, config.Port, config.Encoding);
                        break;
                    case 3:
                        channel = new TcpChannelServerV3(config.RemoteHost, config.Port, config.Encoding);
                        break;

                }
            }
            else
            {
                channel = new TcpChannelClient(config.RemoteHost, config.Port, config.Encoding);
            }
            
            return channel;
        }
    }
}