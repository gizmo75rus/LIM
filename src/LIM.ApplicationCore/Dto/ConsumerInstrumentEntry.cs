using LIM.ApplicationCore.Models;

namespace LIM.ApplicationCore.Dto;

public class ConsumerInstrumentEntry
{
    public Guid Id { get; set; }
    public InstrumentEntry? Device { get; set; }
    public string? DriverVersion { get; set; }
    
    public string? HostAddress { get; set; }
    public int? Port { get; set; }

    public byte? ConnectionType { get; set; }

    public static ConsumerInstrumentEntry Map(ConsumerInstrument entity)
    {
        return new ConsumerInstrumentEntry()
        {
            Id = entity.Id,
            Device = InstrumentEntry.Map(entity.Instrument ?? throw new ArgumentNullException(nameof(entity.Instrument))),
            DriverVersion = entity.DriverVersion,
            Port = entity.Port,
            HostAddress = entity.HostAddress,
            ConnectionType = (byte?)entity.ConnectionType,
            
        };
    }
}