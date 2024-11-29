using LIM.ApplicationCore.Dto;
using LIM.SharedKernel.Interfaces;

namespace LIM.ApplicationCore.Contracts;

public interface IManufacturerService : IService, ILookup
{
    /// <summary>
    /// Создать запись о производителе
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    Task<ManufacturerEntry> Create(string name);
    
    /// <summary>
    /// Переименовать наименование производителя 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="newName"></param>
    /// <returns></returns>
    Task Rename(int id, string newName);
    
    /// <summary>
    /// Удалить запись о производителе
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task Delete(int id);

    /// <summary>
    /// Получить данные о производителе
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<ManufacturerDetail> Detail(int id);
}