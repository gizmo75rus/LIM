using LIM.ApplicationCore.Models;

namespace LIM.ApplicationCore.Dto;

public class ConsumerDetail : ConsumerEntry
{
    public IEnumerable<ConsumerDeviceEntry>? Devices { get; set; }
    public int? DevicesCount { get; set; }

    public new static ConsumerDetail Map(Consumer entity)
    {
        return new ConsumerDetail()
        {
            Id = entity.Id,
            Name = entity.Name,
            Devices = entity.ConsumerDevices?.Select(ConsumerDeviceEntry.Map).ToList(),
            DevicesCount = entity.ConsumerDevices?.Count ?? 0,
        };
    }
}