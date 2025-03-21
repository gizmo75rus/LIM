using LIM.ApplicationCore.BaseObjects;
using LIM.ApplicationCore.Enums;

namespace LIM.ApplicationCore.Models;

/// <summary>
/// Событие устройства
/// </summary>
public class InstrumentEvent : BaseEntity<Guid>
{
    public Guid? ConsumerInstrumentId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public InstrumentEventType EventType { get; set; } = InstrumentEventType.Inactive;
    
    public string? Notes { get; set; }
    public ConsumerInstrument? ConsumerInstrument { get; set; }
}