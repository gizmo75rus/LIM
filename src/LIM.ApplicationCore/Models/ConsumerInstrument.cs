using LIM.ApplicationCore.BaseObjects;
using LIM.ApplicationCore.Enums;

namespace LIM.ApplicationCore.Models;

/// <summary>
/// Лабораторный инструмент потребителя
/// </summary>
public class ConsumerInstrument : JournaledEntity<Guid>
{
    public int? ConsumerId { get; set; }
    public int? InstrumentId { get; set; }
    
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

    /// <summary>
    /// Удалено
    /// </summary>
    public bool Removed { get; set; }
    public Consumer? Consumer { get; set; }
    public Instrument? Instrument { get; set; }
    
    public HashSet<Research>? Researchs { get; set; }
    
    public HashSet<InstrumentMessage>? Messages { get; set; }
    public HashSet<InstrumentEvent>? Events { get; set; }

    public virtual string LookupName => $"Consumer: '{Consumer?.Name}', Instrument:'{Instrument?.LookupName}', endpoint: '{HostAddress}:{Port}'";

    public override string ToString() => $"{Id}: {Instrument?.LookupName}, SerialNumber: {SerialNumber}, ConnectionType: {ConnectionType}, DriverVersion: {DriverVersion}";
}