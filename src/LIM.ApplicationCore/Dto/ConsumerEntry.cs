using LIM.ApplicationCore.Models;

namespace LIM.ApplicationCore.Dto;

public record ConsumerEntry(int Id, string? Name)
{
    public static ConsumerEntry Map(Consumer entity)
    {
        return new ConsumerEntry(entity.Id, entity.Name);
    }
}