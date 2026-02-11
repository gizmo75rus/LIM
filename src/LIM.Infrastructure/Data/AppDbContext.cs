using System.Reflection;
using LIM.SharedKernel.Extensions;
using LIM.SharedKernel.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Logging;

namespace LIM.Infrastructure.Data;

public partial class AppDbContext : DbContext
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<AppDbContext> _logger;

    public AppDbContext(ICurrentUserService currentUserService,ILogger<AppDbContext> logger, DbContextOptions<AppDbContext> options) : base(options)
    {
        _currentUserService = currentUserService;
        _logger = logger;
    }
    
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAudit();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAudit()
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is IJournaledEntity && 
                        (e.State == EntityState.Added || e.State == EntityState.Modified));

        if (!entries.Any())
            return;

        var now = DateTime.UtcNow;
        var userId = _currentUserService.UserId ?? "system";
        var userName = _currentUserService.UserName ?? "system";
        
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Applying audit for {Count} entities. User: {UserId} ({UserName})", 
                entries.Count(), userId, userName);
        }

        foreach (var entry in entries)
        {
            var entity = (IJournaledEntity)entry.Entity;
            
            if (entry.State == EntityState.Added)
            {
                entity.CreatedAt = now;
                entity.CreatedBy = userName; // или userId - решите что хранить
            }
            
            entity.UpdatedAt = now;
            entity.UpdatedBy = userName;
        }
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseSnakeCaseNamingConvention();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var tableName = entityType.GetTableName();
            tableName = tableName?.ToSnakeCase();
            entityType.SetTableName(tableName);

            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime) ||
                    property.ClrType == typeof(DateTime?))
                {
                    property.SetValueConverter(UtcConverter);
                }
            }
        }
    }

    private static readonly ValueConverter<DateTime, DateTime> UtcConverter =
        new(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
    
    
}