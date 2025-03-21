using LIM.SharedKernel.BaseModels;

namespace LIM.SharedKernel.Interfaces;

public interface ILookup
{
    /// <summary>
    /// Получтиь данные для представления ключ-значение
    /// </summary>
    /// <returns></returns>
    public Task<IEnumerable<Lookup>> GetLookUp();
}