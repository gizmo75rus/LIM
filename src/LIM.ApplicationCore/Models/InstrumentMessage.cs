using LIM.ApplicationCore.BaseObjects;
using LIM.ApplicationCore.Enums;

namespace LIM.ApplicationCore.Models;

public class InstrumentMessage : BaseEntity<Guid>
{
    public Guid? ConsumerInstrumentId { get; set; }
    public DataDirection Direction { get; set; } = DataDirection.Unknown;
    public MessageType Type { get; set; } = MessageType.Unknown;
    public DateTime Received { get; set; } = DateTime.Now;
    public DateTime? AcceptTime { get; set; }
    public Guid? AcceptSystemId { get; set; }
    public string? Body { get; set; }
    public ConsumerInstrument? ConsumerInstrument { get; set; }
}