using LIM.ApplicationCore.Dto;
using LIM.SharedKernel.Interfaces;

namespace LIM.ApplicationCore.Contracts;

public interface IConsumerService : IService, ILookup
{
    Task<ConsumerDetail> Detail(int id);
    Task<ConsumerEntry> Create(string name,string lisVersion);
    Task RemoveDevice(Guid consumerDeviceId);
    Task Rename(int consumerId, string name);
    Task Delete(int id);
}