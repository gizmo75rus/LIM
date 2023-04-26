using Autofac;
using LIM.SharedKernel.Helpers;
using Microsoft.Extensions.Configuration;
using Rebus.Bus;
using Rebus.Config;
using Rebus.Handlers;
using Rebus.Retry.Simple;
using Rebus.Serialization.Custom;
using Rebus.Serialization.Json;
using Rebus.Topic;
using Serilog;

namespace LIM.SharedKernel.Rebus;

public class AutofacRebusModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterRebus((configure, context) =>
            {
                var configuration = context.Resolve<IConfiguration>();
                var logger = Log.ForContext<AutofacRebusModule>();
                var connectionSection = configuration.GetSection("RabbitMQConnection");
                var rabbitConnection = $"amqp://{connectionSection["UserName"]}:{connectionSection["Password"]}@{connectionSection["HostName"]}:{connectionSection["Port"]}";
                var exchangeName = connectionSection["ExchangeName"];
                var exchangeTopicName = connectionSection["ExchangeTopicName"];

                if (!string.IsNullOrEmpty(connectionSection["VirtualHost"]))
                {
                    rabbitConnection = rabbitConnection + "/" + connectionSection["VirtualHost"].Replace("/", "");
                }

                configure
                    .Logging(x => x.Serilog(logger))
                    .Transport(x =>
                    {
                        var builder = x.UseRabbitMq(rabbitConnection, connectionSection["Queue"]);
                        builder.PriorityQueue(10);
                        builder.Prefetch(1);
                        builder.DefaultQueueOptions(options =>
                        {
                            options.SetDurable(true);
                            options.SetAutoDelete(false);
                        });
                        builder.ClientConnectionName("CRMConnection");
                        builder.ExchangeNames(directExchangeName: exchangeName, topicExchangeName: exchangeTopicName);
                    })
                    .Serialization(x =>
                    {
                        var types = AssemblyHelpers.GetImplementations(typeof(RebusMessage));
                        x.UseNewtonsoftJson(JsonInteroperabilityMode.PureJson);
                        x.UseCustomMessageTypeNames()
                            .AddWithShortNames(types)
                            .AllowFallbackToDefaultConvention();
                    })
                    .Events(x =>
                    {
                        x.BeforeMessageSent += BeforeSentPriority;
                    })
                    .Options(x =>
                    {
                        x.SetNumberOfWorkers(0);
                        x.SetMaxParallelism(1);
                        x.Register<ITopicNameConvention>(c => new CustomTopicNameConvention());
                        x.SimpleRetryStrategy(maxDeliveryAttempts: 5, secondLevelRetriesEnabled: true);
                    });

                return configure;
            });

            var handlerType = AssemblyHelpers.GetImplementations(typeof(IHandleMessages))
                .FirstOrDefault(x => x.Namespace != null && !x.Namespace.Contains("Rebus", StringComparison.OrdinalIgnoreCase));

            builder.RegisterHandlersFromAssemblyOf(handlerType);
        }

        private void BeforeSentPriority(IBus bus, Dictionary<string, string> headers, object message, global::Rebus.Pipeline.OutgoingStepContext context)
        {
            var priorityKey = "x-max-priority";

            if (message is RebusMessage rebusMessage)
            {
                if (rebusMessage.Priority == 0)
                    return;

                headers.Add(priorityKey, rebusMessage.Priority.ToString());
            }

            if (message is RebusMessage[] messages)
            {
                if (messages.Length == 0)
                    return;

                var priority = messages[0].Priority;
                headers.Add(priorityKey, priority.ToString());
            }
        }
    }