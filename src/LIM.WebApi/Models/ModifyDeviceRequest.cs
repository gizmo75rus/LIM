using LIM.ApplicationCore.Enums;

namespace LIM.WebApp.Models;

public class ModifyDeviceRequest
{
    public int ManufacturerId { get; set; }
    public ProtocolType Protocol { get; set; }
    public string Model { get; set; } = string.Empty;
}