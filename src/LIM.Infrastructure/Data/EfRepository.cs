
using LIM.SharedKernel.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LIM.Infrastructure.Data;

public class EfRepository : IRepository
{
    private readonly AppDbContext _ctx;

    public EfRepository(AppDbContext ctx)
    {
        _ctx = ctx;
    }

    public IQueryable<T> Record<T>(bool noTracking) where T : class, IEntity
    {
        return noTracking ? _ctx.Set<T>().AsNoTracking().AsQueryable() : _ctx.Set<T>().AsQueryable();
    }

    public void Insert<T>(T entity) where T : class, IEntity
    {
        _ctx.Set<T>().Add(entity);
    }

    public void Insert<T>(IEnumerable<T> collection) where T : class, IEntity
    {
        _ctx.Set<T>().AddRange(collection);
    }

    public void Update<T>(T entity) where T : class, IEntity
    {
        _ctx.Set<T>().Attach(entity);
        _ctx.Entry(entity).State = EntityState.Modified;
    }

    public void Remove<T>(T entity) where T : class, IEntity
    {
        _ctx.Set<T>().Remove(entity);
    }

    public async Task SaveChangeAsync()
    {
        await _ctx.SaveChangesAsync();
    }

    public void SaveChange()
    {
        _ctx.SaveChangesAsync();
    }
}