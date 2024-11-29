namespace LIM.SharedKernel.Interfaces;

public interface IRepository
{
    IQueryable<T> Record<T>() where T : class,IEntity;
    Task<int> AddAsync<T>(T entity, CancellationToken ct = default) where T : class, IEntity;
    Task<int> AddAsync<T>(IEnumerable<T> collection, CancellationToken ct = default) where T : class, IEntity;
    Task<int> UpdateAsync<T>(T entity, CancellationToken ct = default) where T : class, IEntity;
    Task<int> DeleteAsync<T>(T entity, CancellationToken ct = default) where T : class, IEntity;
}