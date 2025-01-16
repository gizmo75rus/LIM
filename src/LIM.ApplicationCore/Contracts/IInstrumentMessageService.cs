using LIM.ApplicationCore.Enums;
using LIM.SharedKernel.Interfaces;

namespace LIM.ApplicationCore.Contracts;

public interface IInstrumentMessageService : IService
{
    Task AddAsync(Guid consumerInstrumentId, DataDirection direction, string body);
}