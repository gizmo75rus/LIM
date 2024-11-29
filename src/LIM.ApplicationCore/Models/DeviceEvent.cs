using LIM.ApplicationCore.BaseObjects;

namespace LIM.ApplicationCore.Models;

/// <summary>
/// Событие устройства
/// </summary>
public class DeviceEvent : BaseEntity<Guid>
{
    public Guid? ConsumerDeviceId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Message { get; set; }
    public ConsumerDevice? ConsumerDevice { get; set; }
}