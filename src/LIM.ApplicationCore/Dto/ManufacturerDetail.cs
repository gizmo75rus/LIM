using LIM.ApplicationCore.Models;

namespace LIM.ApplicationCore.Dto;

public class ManufacturerDetail : ManufacturerEntry
{
    public IEnumerable<DeviceEntry>? Devices { get; set; }
    
    public int? DevicesCount { get; set; }

    public new static ManufacturerDetail Map(Manufacturer manufacturer)
    {
        return new ManufacturerDetail
        {
            Id = manufacturer.Id,
            Name = manufacturer.Name,
            Devices = manufacturer?.Devices?.Select(DeviceEntry.Map),
            DevicesCount = manufacturer?.Devices?.Count,
        };
    }
}