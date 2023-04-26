using System.Net.Sockets;
using LIM.ApplicationCore.Entities;

namespace LIM.ApplicationCore.Interfaces;

public interface IDeviceService
{
    Task<Device> CreateDevice(string manufacturer, string model, ProtocolType protocol);
}