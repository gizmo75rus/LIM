using LIM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LIM.Infrastructure.ServiceConfiguration;
public static class DataBaseConfigurations
{
    /// <summary>
    /// Регистрирует контекст БД с PostgreSQL (Npgsql)
    /// </summary>
    public static IServiceCollection AddPostgresDbContext(
        this IServiceCollection services, 
        IConfiguration configuration,
        string connectionStringName = "AppDbContext")
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString(connectionStringName)));

        return services;
    }
    
    /// <summary>
    /// Инициализирует БД (только для разработки/тестов!)
    /// </summary>
    public static void EnsureDbCreated(this IServiceProvider provider)
    {
        using var scope = provider.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        ctx.Database.EnsureCreated();
    }
    
    /// <summary>
    /// Инициализирует БД (только для разработки/тестов!)
    /// </summary>
    public static async Task EnsureDbCreatedAsync(this IServiceProvider provider)
    {
        using var scope = provider.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await ctx.Database.EnsureCreatedAsync();
    }
    
    /// <summary>
    /// Инициализирует БД (только для разработки/тестов!)
    /// (Drop -> Create)
    /// </summary>
    public static async Task EnsureDbDropCreatedAsync(this IServiceProvider provider)
    {
        using var scope = provider.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await ctx.Database.EnsureDeletedAsync();
        await ctx.Database.EnsureCreatedAsync();
    }
    
    /// <summary>
    /// Применяет миграции к БД (для продакшена)
    /// </summary>
    public static async Task MigrateDbContextAsync(this IServiceProvider provider)
    {
        using var scope = provider.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await ctx.Database.MigrateAsync();
    }
    
    /// <summary>
    /// InMemory БД для тестов
    /// </summary>
    /// <param name="services"></param>
    /// <param name="databaseName"></param>
    /// <returns></returns>
    public static IServiceCollection AddInMemoryDbContext(
        this IServiceCollection services,
        string databaseName = "AppInMemoryDb")
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(databaseName));
    
        return services;
    }
}