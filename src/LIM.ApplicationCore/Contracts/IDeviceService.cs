using LIM.ApplicationCore.Dto;
using LIM.ApplicationCore.Enums;
using LIM.SharedKernel.Interfaces;

namespace LIM.ApplicationCore.Contracts;

public interface IDeviceService : IService, ILookup
{
    Task<DeviceEntry> Detail(int deviceId);
    Task<DeviceEntry> Create(string manufacturer, string model, ProtocolType protocol);
    Task Modify(int deviceId, int manufacturerId, string model, ProtocolType protocol);
    Task Delete(int deviceId);
}