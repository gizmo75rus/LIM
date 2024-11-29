using LIM.ApplicationCore.Models;

namespace LIM.ApplicationCore.Dto;

public class ConsumerDeviceEntry
{
    public Guid Id { get; set; }
    public DeviceEntry? Device { get; set; }
    public string? DriverVersion { get; set; }
    
    public string? HostAddress { get; set; }
    public int? Port { get; set; }

    public byte? ConnectionType { get; set; }

    public static ConsumerDeviceEntry Map(ConsumerDevice entity)
    {
        return new ConsumerDeviceEntry()
        {
            Id = entity.Id,
            Device = DeviceEntry.Map(entity.Device ?? throw new ArgumentNullException(nameof(entity.Device))),
            DriverVersion = entity.DriverVersion,
            Port = entity.Port,
            HostAddress = entity.HostAddress,
            ConnectionType = (byte?)entity.ConnectionType,
            
        };
    }
}