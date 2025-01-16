using LIM.ApplicationCore.Models;

namespace LIM.ApplicationCore.Dto;

public record ManufacturerEntry(int Id, string? Name)
{
    public static ManufacturerEntry Map(Manufacturer manufacturer)
    {
        return new ManufacturerEntry(manufacturer.Id, manufacturer.Name);
    }
}