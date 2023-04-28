using Autofac;
using LIM.ApplicationCore;
using LIM.SharedKernel.Interfaces;

namespace LIM.Infrastructure;

public class ServiceModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        base.Load(builder);

        builder.RegisterAssemblyTypes(ApplicationCoreAssembly.Value);
        
        builder
            .RegisterAssemblyTypes(AppDomain.CurrentDomain.GetAssemblies())
            .AssignableTo<IService>()
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope();
        
        builder
            .RegisterAssemblyTypes(AppDomain.CurrentDomain.GetAssemblies())
            .AssignableTo<IRepository>()
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope();
    }
}