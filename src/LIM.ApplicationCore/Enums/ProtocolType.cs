using System.Text.Json.Serialization;

namespace LIM.ApplicationCore.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ProtocolType : byte
{
    ASTM = 0,
    HL7 = 1,
    BC = 2,
    CP = 3
}