using AstmDriver.Abstractions;
using AstmDriver.Behavior.MessageWorker;
using AstmDriver.Commons;
using AstmDriver.Exceptions;

namespace AstmDriver.Core
{
    public static class MessageWorkerFactory
    {
        public static IMessageWorker Create(DriverConfig config,IHost host)
        {
            IMessageWorker worker = null;
            var analyzer = Dal.GetAnalyzer(config.RemoteHost, config.Port);

            switch (config.MessageWorker)
            {
                case "DefaultAstm":
                    worker = new Astm_Worker(host, LoggerFactory.GetLogger(typeof(Astm_Worker)), analyzer);
                    break;
                case "DefaultHl7":
                    worker = new HL7_Worker(host, config, LoggerFactory.GetLogger(typeof(HL7_Worker)), analyzer);
                    break;
                case "Saphir400":
                    worker = new Saphir400_Worker(host, LoggerFactory.GetLogger(typeof(Saphir400_Worker)), analyzer);
                    break;
                case "AU480":
                    worker = new AU480_Worker(host, LoggerFactory.GetLogger(typeof(AU480_Worker)), analyzer);
                    break;
                case "Metascan":
                    worker = new Metascan(host, LoggerFactory.GetLogger(typeof(Metascan)), analyzer);
                    break;
                case "Miura":
                    worker = new Miura_Worker(host, LoggerFactory.GetLogger(typeof(Miura_Worker)), analyzer);
                    break;
                case "Aspect":
                    worker = new MedicalSystem_Worker(host, LoggerFactory.GetLogger(typeof(MedicalSystem_Worker)), analyzer);
                    break;
                default:
                    throw new ArgumentConfigException("Не верный параметр конфигурации DeviceConfig.MessageWorker");
            }

            return worker;
        }
    }
}
