using LIM.ApplicationCore.Models;

namespace LIM.ApplicationCore.Dto;

public class ConsumerEntry
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public static ConsumerEntry Map(Consumer entity)
    {
        return new ConsumerEntry()
        {
            Id = entity.Id,
            Name = entity.Name
        };
    }
}