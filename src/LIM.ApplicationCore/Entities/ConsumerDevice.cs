using LIM.ApplicationCore.BaseObjects;
using LIM.ApplicationCore.Enums;

namespace LIM.ApplicationCore.Entities;

/// <summary>
/// Лабораторный инструмент потребителя
/// </summary>
public class ConsumerDevice : JournaledEntity<Guid>
{
    public int? ConsumerId { get; set; }
    public int? DeviceId { get; set; }
    
    /// <summary>
    /// Серийный номер
    /// </summary>
    public string? SerialNumber { get; set; }
    
    /// <summary>
    /// Год выпуска
    /// </summary>
    public int BuildYear { get; set; }
    
    /// <summary>
    /// Тип подключения
    /// </summary>
    public ConnectionType ConnectionType { get; set; }

    /// <summary>
    /// Версия драйвера
    /// </summary>
    public string? DriverVersion { get; set; }
    
    /// <summary>
    /// Адрес хоста
    /// </summary>
    public string? HostAddress { get; set; }
    
    /// <summary>
    /// Порт
    /// </summary>
    public short Port { get; set; }
    public Consumer? Consumer { get; set; }
    public Device? Device { get; set; }
    
    public HashSet<Test>? Tests { get; set; }
    public HashSet<DeviceEvent>? DeviceEvents { get; set; }
    
}