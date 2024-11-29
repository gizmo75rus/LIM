namespace LIM.SharedKernel.Interfaces;

public interface ILookup
{
    /// <summary>
    /// Получтиь данные для представления ключ-значение
    /// </summary>
    /// <returns></returns>
    public Task<Dictionary<int, string?>> GetLookUp();
}