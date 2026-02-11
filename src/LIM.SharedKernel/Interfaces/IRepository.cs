namespace LIM.SharedKernel.Interfaces;

/// <summary>
/// Базовый репозиторий
/// </summary>
public interface IRepository
{
    /// <summary>
    /// Получить объект запроса IQueryable для типа T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    IQueryable<T> Record<T>() where T : class,IEntity;
    
    /// <summary>
    /// Добавить запись в БД
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="ct"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns>вставленное кол-во строк в БД</returns>
    Task<int> AddAsync<T>(T entity, CancellationToken ct = default) where T : class, IEntity;
    
    /// <summary>
    /// Добавить несколько записей в БД
    /// </summary>
    /// <param name="collection"></param>
    /// <param name="ct"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns>вставленоое кол-во строк в БД</returns>
    Task<int> AddAsync<T>(IEnumerable<T> collection, CancellationToken ct = default) where T : class, IEntity;
    
    /// <summary>
    /// Обновить запись
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="ct"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns>кол-во изменных строк</returns>
    Task<int> UpdateAsync<T>(T entity, CancellationToken ct = default) where T : class, IEntity;
    
    /// <summary>
    /// Удалить запись
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="ct"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns>кол-во удаленных строк</returns>
    Task<int> DeleteAsync<T>(T entity, CancellationToken ct = default) where T : class, IEntity;
}