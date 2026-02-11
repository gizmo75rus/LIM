using LIM.SharedKernel.BaseModels;

namespace LIM.SharedKernel.Interfaces;

/// <summary>
/// Контракт справочника
/// </summary>
public interface ILookup
{
    /// <summary>
    /// Получтиь данные справочника
    /// </summary>
    /// <returns></returns>
    public Task<IEnumerable<Lookup>> GetLookup();
}