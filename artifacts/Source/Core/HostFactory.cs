using AstmDriver.Abstractions;
using AstmDriver.Behavior.Host;
using AstmDriver.Commons;
using AstmDriver.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstmDriver.Core
{
    public static class HostFactory
    {
        public static IHost Create(DriverConfig config, ITransportChannel channel)
        {
            IHost host = null;
            switch (config.HostType)
            {
                case nameof(AstmHost):
                    host = new AstmHost(channel,LoggerFactory.GetLogger(typeof(AstmHost)));
                    break;
                case nameof(BeckmanCoulterHost):
                    host = new BeckmanCoulterHost(channel, LoggerFactory.GetLogger(typeof(BeckmanCoulterHost)));
                    break;
                case nameof(MetascanHost):
                    host = new MetascanHost(channel, LoggerFactory.GetLogger(typeof(MetascanHost)));
                    break;
                case nameof(MiuraHost):
                    host = new MiuraHost(channel, LoggerFactory.GetLogger(typeof(MiuraHost)));
                    break;
                case nameof(MllpHost):
                    host = new MllpHost(channel, LoggerFactory.GetLogger(typeof(MllpHost)));
                    break;
                default:
                    throw new ArgumentConfigException($"Не верный параметр конфигурации {nameof(config.HostType)} переданное значение:{config.HostType}");
            }
            return host;
        }
    }
}
