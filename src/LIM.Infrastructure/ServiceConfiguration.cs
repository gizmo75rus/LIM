using LIM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LIM.Infrastructure;

public static class ServiceConfigurationExtensions
{
    public static IServiceCollection AddNpgDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString(nameof(AppDbContext))));

        return services;
    }
    
    public static IServiceProvider InitDbContext(this IServiceProvider provider)
    {
        using var serviceScope = provider.CreateScope();
        var ctx = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();
   //     ctx.Database.EnsureDeleted();
        ctx.Database.EnsureCreated();
        return provider;
    }
    
    // public static IServiceCollection AddInMemoryDbContext(this IServiceCollection services)
    // {
    //     services.AddDbContext<AppDbContext>(options =>
    //         options.UseInMemoryDatabase("mem_db"));
    //
    //     return services;
    // }
}