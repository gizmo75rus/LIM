using LIM.ApplicationCore.Dto;
using LIM.ApplicationCore.Enums;

namespace LIM.WebApp.Models;

public class ConsumerAddInstrumentRequest
{
    public int ConsumerId { get; set; }
    
    public int InstrumentId { get; set; }
    public short Port { get; set; }
    public string HostAddress { get; set; } = String.Empty;
    
    public string DriverVersion { get; set; } = string.Empty;
    public ConnectionType ConnectionType { get; set; }
}