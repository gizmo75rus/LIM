using LIM.ApplicationCore.Entities;

namespace LIM.ApplicationCore.Interfaces;

public interface IConsumerService
{
    Task<Consumer> CreateConsumer(string name,string lisVersion);
}