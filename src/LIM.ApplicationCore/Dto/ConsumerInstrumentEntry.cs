using LIM.ApplicationCore.Models;

namespace LIM.ApplicationCore.Dto;

public record ConsumerInstrumentEntry(Guid Id, InstrumentEntry? Device, string? DriverVersion, string? HostAddress, int? Port, byte? ConnectionType)
{
    public static ConsumerInstrumentEntry Map(ConsumerInstrument entity)
    {
        return new ConsumerInstrumentEntry(
            Id: entity.Id,
            Device: InstrumentEntry.Map(entity.Instrument ?? throw new ArgumentNullException(nameof(entity.Instrument))),
            DriverVersion: entity.DriverVersion,
            Port: entity.Port,
            HostAddress: entity.HostAddress,
            ConnectionType: (byte?)entity.ConnectionType);
    }
}