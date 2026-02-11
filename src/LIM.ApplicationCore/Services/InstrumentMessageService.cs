using LIM.ApplicationCore.BaseObjects;
using LIM.ApplicationCore.Contracts;
using LIM.ApplicationCore.Enums;
using LIM.ApplicationCore.Exceptions;
using LIM.ApplicationCore.Models;
using LIM.SharedKernel.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LIM.ApplicationCore.Services;

public class InstrumentMessageService : AbstractService, IInstrumentMessageService
{
    private readonly IRepository _repository;

    public InstrumentMessageService(ILogger<InstrumentMessageService> logger, IRepository repository)
    {
        _logger = logger;
        _repository = repository;
    }

    public async Task AddAsync(Guid consumerInstrumentId, DataDirection direction,  string body)
    {
        _logger.LogInformation($"Begin receive message for consumer instrument id:'{consumerInstrumentId}'");
        
        if(false == await _repository
               .Record<ConsumerInstrument>()
               .AnyAsync(x=> x.Id == consumerInstrumentId, _cts.Token))
            throw CommonException.NotFound;
            
        var message = new InstrumentMessage
        {
            ConsumerInstrumentId = consumerInstrumentId,
            Direction = direction,
            Body = body
        };

        if (1 < await _repository.AddAsync(message, _cts.Token))
            throw CommonException.FailedToSaveObject;
        
        _logger.LogInformation($"Message added to database. Message id: {message.Id}");
    }
}