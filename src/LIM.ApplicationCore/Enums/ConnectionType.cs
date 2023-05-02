using System.Text.Json.Serialization;

namespace LIM.ApplicationCore.Enums;

/// <summary>
/// типы подключения
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ConnectionType : byte
{
    Client,
    Server
}