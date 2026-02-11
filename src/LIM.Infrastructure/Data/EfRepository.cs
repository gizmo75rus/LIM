
using LIM.SharedKernel.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;

namespace LIM.Infrastructure.Data;

/// <inheritdoc />
/// <remarks>Реализация репозитория для EF Core</remarks>
public class EfRepository : IRepository
{
    private readonly AppDbContext _ctx;
    private readonly ILogger _logger;

    public EfRepository(AppDbContext ctx, ILogger<EfRepository> logger)
    {
        _ctx = ctx;
        _logger = logger;
    }

    /// <inheritdoc />
    public IQueryable<T> Record<T>() where T : class, IEntity
    {
        return _ctx.Set<T>().AsNoTracking().AsQueryable();
    }

    /// <inheritdoc />
    public Task<int> AddAsync<T>(T entity, CancellationToken ct) where T : class, IEntity
    {
        _logger.LogInformation("Adding {entity}", entity);
        _ctx.Set<T>().Add(entity);
        return _ctx.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public Task<int> AddAsync<T>(IEnumerable<T> collection, CancellationToken ct) where T : class, IEntity
    {
        _logger.LogInformation("Adding {collection}", collection);
        _ctx.Set<T>().AddRange(collection);
        return _ctx.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public Task<int> UpdateAsync<T>(T entity, CancellationToken ct) where T : class, IEntity
    {
        if (!_ctx.Set<T>().Local.Contains(entity))
        {
            _ctx.Set<T>().Attach(entity);
        }
        _ctx.Entry(entity).State = EntityState.Modified;
        return _ctx.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public Task<int> DeleteAsync<T>(T entity, CancellationToken ct) where T : class, IEntity
    {
        _ctx.Set<T>().Remove(entity);
        return _ctx.SaveChangesAsync(ct);
    }
}