namespace LIM.ApplicationCore.Enums;

public enum InstrumentEventType : byte
{
    Inactive = 0,
    Handshake = 1,
    Connection = 2,
    Idle = 3,
    Busy = 4,
    Working = 5,
    Testing = 6,
    Error = 7,
}