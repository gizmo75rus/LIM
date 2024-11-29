using LIM.ApplicationCore.Models;

namespace LIM.ApplicationCore.Dto;

public class ManufacturerEntry
{
    public int Id { get; set; }
    public string? Name { get; set; }

    public static ManufacturerEntry Map(Manufacturer manufacturer)
    {
        return new ManufacturerDetail
        {
            Id = manufacturer.Id,
            Name = manufacturer.Name,
        };
    }
}