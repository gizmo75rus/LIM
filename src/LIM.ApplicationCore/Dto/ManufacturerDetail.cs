using LIM.ApplicationCore.Models;

namespace LIM.ApplicationCore.Dto;

public class ManufacturerDetail : ManufacturerEntry
{
    public IEnumerable<InstrumentEntry>? Instruments { get; set; }
    
    public int? InstrumentsCount { get; set; }

    public new static ManufacturerDetail Map(Manufacturer manufacturer)
    {
        return new ManufacturerDetail
        {
            Id = manufacturer.Id,
            Name = manufacturer.Name,
            Instruments = manufacturer?.Instruments?.Select(InstrumentEntry.Map),
            InstrumentsCount = manufacturer?.Instruments?.Count,
        };
    }
}