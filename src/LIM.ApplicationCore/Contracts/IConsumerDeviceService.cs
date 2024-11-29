using LIM.ApplicationCore.Dto;
using LIM.ApplicationCore.Enums;
using LIM.SharedKernel.Interfaces;

namespace LIM.ApplicationCore.Contracts;

public interface IConsumerDeviceService : IService
{
    Task<Dictionary<Guid, string?>> GetLookUp(int customerId);
    Task<ConsumerDeviceEntry> AddDevice(int consumerId, int deviceId, string hostAddress, short port, ConnectionType connectionType, string driverVersion);
}