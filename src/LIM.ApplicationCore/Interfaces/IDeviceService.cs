using System.Net.Sockets;
using LIM.ApplicationCore.Entities;

namespace LIM.ApplicationCore.Interfaces;

public interface IDeviceService
{
    Task<Dictionary<int, string?>> GetLookUp();
    Task<Device> Get(string manufacturer, string model);
    Task<Device> Create(string manufacturer, string model, ProtocolType protocol);
    Task Update(Device device);
}