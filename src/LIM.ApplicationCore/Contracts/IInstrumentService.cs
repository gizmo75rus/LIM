using LIM.ApplicationCore.Dto;
using LIM.ApplicationCore.Enums;
using LIM.SharedKernel.Interfaces;

namespace LIM.ApplicationCore.Contracts;

public interface IInstrumentService : IService, ILookup
{
    Task<InstrumentEntry> Detail(int deviceId);
    Task<InstrumentEntry> Create(string manufacturer, string model, ProtocolType protocol);
    Task Modify(int deviceId, int manufacturerId, string model, ProtocolType protocol);
    Task Delete(int deviceId);
}