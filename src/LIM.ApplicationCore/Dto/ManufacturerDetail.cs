using LIM.ApplicationCore.Models;

namespace LIM.ApplicationCore.Dto;

public record ManufacturerDetail(int Id, string? Name, IEnumerable<InstrumentEntry>? Instruments, int? InstrumentsCount) 
{
    public new static ManufacturerDetail Map(Manufacturer manufacturer)
    {
        return new ManufacturerDetail(Id: manufacturer.Id,
            Name: manufacturer.Name,
            Instruments: manufacturer?.Instruments?.Select(InstrumentEntry.Map),
            InstrumentsCount: manufacturer?.Instruments?.Count);
    }
}