using LIM.ApplicationCore.Dto;
using LIM.ApplicationCore.Enums;
using LIM.SharedKernel.Interfaces;

namespace LIM.ApplicationCore.Contracts;

public interface IConsumerInstrumentService : IService
{
    Task<Dictionary<Guid, string?>> GetLookUp(int consumerId);
    Task<ConsumerInstrumentEntry> AddInstrument(int consumerId, int instrumentId, string hostAddress, short port, ConnectionType connectionType, string driverVersion);
    Task AddEvent(Guid consumerInstrumentId, InstrumentEventType eventType, string? notes = null);
}