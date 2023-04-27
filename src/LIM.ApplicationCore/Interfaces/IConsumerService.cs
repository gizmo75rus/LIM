using LIM.ApplicationCore.Entities;

namespace LIM.ApplicationCore.Interfaces;

public interface IConsumerService
{
    Task<Consumer> Create(string name,string lisVersion);
    Task<Dictionary<int, string?>> GeLookup();
    Task<Consumer> Get(int Id);
    Task AddDevice(Consumer consumer, Device device);
    Task RemoveDevice(Consumer consumer, Device device);
}