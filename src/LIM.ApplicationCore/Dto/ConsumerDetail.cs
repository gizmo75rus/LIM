using LIM.ApplicationCore.Models;

namespace LIM.ApplicationCore.Dto;

public class ConsumerDetail : ConsumerEntry
{
    public IEnumerable<ConsumerInstrumentEntry>? Devices { get; set; }
    public int? DevicesCount { get; set; }

    public new static ConsumerDetail Map(Consumer entity)
    {
        return new ConsumerDetail()
        {
            Id = entity.Id,
            Name = entity.Name,
            Devices = entity.Instruments?.Select(ConsumerInstrumentEntry.Map).ToList(),
            DevicesCount = entity.Instruments?.Count ?? 0,
        };
    }
}