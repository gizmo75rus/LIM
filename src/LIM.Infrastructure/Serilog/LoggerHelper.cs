using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;

namespace LIM.Infrastructure.Serilog;

public static class LoggerHelper
    {
        /// <summary>
        /// Добавление логгера в проект
        /// </summary>
        /// <param name="applicationName">Наименование приложения, по данному параметру группируются логи</param>
        /// <remarks>
        /// Пример настройки файла конфигурации:
        ///   "Logging": {
        ///        "LogLevelType": {
        ///          "Console": "Information",
        ///          "File": "Warning",
        ///          "RabbitMQ": "Warning"
        ///         },
        ///        "Overrides": {
        ///          "Microsoft": "Information",
        ///          "Microsoft.AspNetCore": "Warning",
        ///          "Microsoft.AspNetCore.SignalR": "Information",
        ///          "Microsoft.AspNetCore.Http.Connections": "Information"
        ///        },
        ///    Если необходимо удалить ключ из словаря "Overrides" в словаре по умолчанию
        ///    нужно описать этот ключ со значением null
        /// </remarks>
        public static void AddLogger(string applicationName)
        {
            var configuration = ConfigurationHelper.CreateConfiguration();

            var logPath = Path.Combine(Directory.GetCurrentDirectory(), "logs", "log.txt");

            var consoleLevel = GetLevel(configuration["Logging:LogLevelType:Console"], LogEventLevel.Information);
            var fileLevel = GetLevel(configuration["Logging:LogLevelType:File"]);
            //var rabbitMqLevel = GetLevel(configuration["Logging:LogLevelType:RabbitMQ"]);

            var defaultOverrides = GetOverrides(configuration);

            var loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Debug();

            foreach (var @override in defaultOverrides)
            {
                loggerConfiguration
                    .MinimumLevel.Override(@override.Key, @override.Value);
            }

            loggerConfiguration
                .Enrich.WithProperty("Application", applicationName)
                .Enrich.FromLogContext()
                .WriteTo.Console(consoleLevel)
                .WriteTo.File(logPath, fileLevel, rollingInterval: RollingInterval.Day);
                //.WriteTo.RabbitMQ((clientConfiguration, sinkConfiguration) =>
                //{
                //    clientConfiguration.Username = configuration["RabbitMQConnection:UserName"];
                //    clientConfiguration.Password = configuration["RabbitMQConnection:Password"];
                //    clientConfiguration.Exchange = configuration["EventBusOptions:ExchangeName"];
                //    clientConfiguration.VHost = configuration["RabbitMQConnection:VirtualHost"];
                //    clientConfiguration.ExchangeType = ExchangeType.Direct;
                //    clientConfiguration.DeliveryMode = RabbitMQDeliveryMode.Durable;
                //    clientConfiguration.RouteKey = "LogEvent";
                //    clientConfiguration.Port = Convert.ToInt32(configuration["RabbitMQConnection:Port"]);

                //    clientConfiguration.Hostnames.Add(configuration["RabbitMQConnection:HostName"]);
                //    sinkConfiguration.TextFormatter = new JsonFormatter();
                //    sinkConfiguration.RestrictedToMinimumLevel = rabbitMqLevel;
                //});


            Log.Logger = loggerConfiguration.CreateLogger();
        }

        private static Dictionary<string, LogEventLevel> GetOverrides(IConfiguration configuration)
        {
            var defaultOverrides = new Dictionary<string, LogEventLevel>()
            {
                { "Microsoft", LogEventLevel.Information },
                { "Microsoft.AspNetCore", LogEventLevel.Warning },
                { "Microsoft.AspNetCore.SignalR", LogEventLevel.Information },
                { "Microsoft.AspNetCore.Http.Connections", LogEventLevel.Information }
            };

            var overrides = configuration.GetSection("Logging:Overrides").GetChildren();
            if (overrides.Any())
                return defaultOverrides;

            foreach (var @override in overrides)
            {
                if (defaultOverrides.ContainsKey(@override.Key))
                {
                    if (string.IsNullOrEmpty(@override.Value))
                        defaultOverrides.Remove(@override.Key);
                    else
                        defaultOverrides[@override.Key] = GetLevel(@override.Value);
                }
                else
                {
                    defaultOverrides.Add(@override.Key, GetLevel(@override.Value));
                }
            }

            return defaultOverrides;
        }

        private static LogEventLevel GetLevel(string name, LogEventLevel @default = LogEventLevel.Warning)
        {
            if (Enum.TryParse<LogEventLevel>(name, true, out var result))
            {
                return result;
            }
            else
            {
                return @default;
            }
        }
    }