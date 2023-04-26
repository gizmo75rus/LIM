namespace LIM.SharedKernel.Interfaces;

public interface IRepository
{
    IQueryable<T> Record<T>(bool noTracking) where T : class,IEntity;
    void Insert<T>(T entity) where T : class, IEntity;
    void Insert<T>(IEnumerable<T> collection) where T : class, IEntity;
    void Update<T>(T entity) where T : class, IEntity;
    void Remove<T>(T entity) where T : class, IEntity;
    Task SaveChangeAsync();
    void SaveChange();
}