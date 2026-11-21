using AstmDriver.Abstractions;
using System;

namespace AstmDriver.Core
{
    public static class LoggerFactory
    {
        private static ILogger _logger;

        public static void CreateLogger(string loggerType, int logLevel)
        {
            switch (loggerType)
            {
                case "File":
                    _logger = new FileLogger(logLevel);
                    break;
                default:
                    throw new NotImplementedException("Указанный тип логгера не реализован");
            }
        }
        internal static ILogger GetLogger(Type type)
        {
            return _logger;
        }
    }
}
