using LIM.ApplicationCore.Enums;
using LIM.ApplicationCore.Models;

namespace LIM.ApplicationCore.Dto;

public record InstrumentEntry(int Id, int? ManufacturerId, string? Manufacturer, string? Model, ProtocolType? Protocol)
{
    public static InstrumentEntry Map(Instrument entity)
    {
        return new InstrumentEntry(Id: entity.Id,
            Model: entity.Model,
            Protocol: entity.ProtocolType,
            ManufacturerId: entity.ManufacturerId,
            Manufacturer: entity?.Manufacturer?.Name);
    }
}