using LIM.ApplicationCore.BaseObjects;

namespace LIM.ApplicationCore.Entities;

/// <summary>
/// Потребитель
/// </summary>
public class Consumer : BaseEntity<int>
{
    /// <summary>
    /// Наименование
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// версия ЛИС
    /// </summary>
    public string? LisVersion { get; set; }
    
    /// <summary>
    /// Инструменты
    /// </summary>
    public HashSet<ConsumerDevice>? ConsumerDevices { get; set; }

}