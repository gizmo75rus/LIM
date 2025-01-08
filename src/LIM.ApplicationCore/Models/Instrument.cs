using LIM.ApplicationCore.BaseObjects;
using LIM.ApplicationCore.Enums;
using LIM.SharedKernel.Interfaces;

namespace LIM.ApplicationCore.Models;

/// <summary>
/// Лабораторный инструмент (анализатор, устройство...)
/// </summary>
public class Instrument : BaseEntity<int>
{
    public int? ManufacturerId { get; set; }
    /// <summary>
    /// Модель
    /// </summary>
    public string? Model { get; set; }
    
    /// <summary>
    /// протокол работы
    /// </summary>
    public ProtocolType ProtocolType { get; set; }
    
    /// <summary>
    /// Производитель
    /// </summary>
    public Manufacturer? Manufacturer { get; set; }

    /// <summary>
    /// потребители
    /// </summary>
    public HashSet<ConsumerInstrument>? ConsumerDevices { get; set; }

    public virtual string LookupName => $"{Manufacturer?.Name} {Model}";
}