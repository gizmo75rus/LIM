using LIM.ApplicationCore.Models;

namespace LIM.ApplicationCore.Dto;

public record ConsumerDetail(int Id, string? Name, IEnumerable<ConsumerInstrumentEntry>? Devices, int? DevicesCount) 
{
    public new static ConsumerDetail Map(Consumer entity)
    {
        return new ConsumerDetail(
            Id: entity.Id,
            Name: entity.Name,
            Devices: entity.Instruments?.Select(ConsumerInstrumentEntry.Map).ToList(),
            DevicesCount: entity.Instruments?.Count ?? 0);
    }
}