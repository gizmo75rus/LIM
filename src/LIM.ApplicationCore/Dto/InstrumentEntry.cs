using LIM.ApplicationCore.Enums;
using LIM.ApplicationCore.Models;

namespace LIM.ApplicationCore.Dto;

public class InstrumentEntry
{
    public int Id { get; set; }
    public int? ManufacturerId { get; set; }
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public ProtocolType? Protocol { get; set; }

    public static InstrumentEntry Map(Instrument entity)
    {
        return new InstrumentEntry()
        {
            Id = entity.Id,
            Model = entity.Model,
            Protocol = entity.ProtocolType,
            ManufacturerId = entity.ManufacturerId,
            Manufacturer = entity?.Manufacturer?.Name
        };
    }
}