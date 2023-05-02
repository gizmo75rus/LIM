using LIM.ApplicationCore.Entities;
using LIM.ApplicationCore.Enums;

namespace LIM.ApplicationCore.Interfaces;

public interface IDeviceService
{
    Task<Dictionary<int, string>> GetLookUp();
    Task<Device> Get(string manufacturer, string model);
    Task<Device> Create(string manufacturer, string model, ProtocolType protocol);
    Task Update(int deviceId, string manufacturerName, string model, ProtocolType protocol);
    
}