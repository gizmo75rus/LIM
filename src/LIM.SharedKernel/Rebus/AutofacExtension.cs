using Autofac;
using LIM.SharedKernel.Helpers;
using Rebus.Bus;
using Rebus.Handlers;

namespace LIM.SharedKernel.Rebus;

public static class AutofacExtension
{
    public static async Task UseRebus(this ILifetimeScope lifetimeScope)
    {
        var bus = lifetimeScope.Resolve<IBus>();
        var messageTypes = AssemblyHelpers.GetImplementations(typeof(IHandleMessages))
            .SelectMany(x => x.GetInterfaces())
            .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IHandleMessages<>))
            .SelectMany(x => x.GetGenericArguments())
            .Where(x => typeof(RebusMessage).IsAssignableFrom(x))
            .ToList();

        foreach (var messageType in messageTypes)
        {
            await bus.Subscribe(messageType);
        }

        bus.Advanced.Workers.SetNumberOfWorkers(1);
    }
}