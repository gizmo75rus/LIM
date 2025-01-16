using LIM.ApplicationCore.BaseObjects;
using LIM.ApplicationCore.Contracts;
using LIM.ApplicationCore.Dto;
using LIM.ApplicationCore.Enums;
using LIM.ApplicationCore.Exceptions;
using LIM.ApplicationCore.Models;
using LIM.SharedKernel.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LIM.ApplicationCore.Services;

public class ConsumerInstrumentService : AbstractService, IConsumerInstrumentService
{
    private readonly IRepository _repository;
    
    public ConsumerInstrumentService(ILogger<ConsumerInstrumentService> logger, IRepository repository)
    {
        _logger = logger;
        _repository = repository;
    }
    
    public async Task<ConsumerInstrumentEntry> AddInstrument(int consumerId, int instrumentId, string hostAddress, short port, ConnectionType connectionType, string driverVersion)
    {
        if(consumerId == int.MaxValue || instrumentId == int.MaxValue || port == short.MaxValue)
            throw CommonException.ArgumentOutException;
            
        _logger.LogInformation($"Add device to consumer [consumerId:{consumerId}, deviceId:{instrumentId}, hostAddress:{hostAddress}, port:{port}, connectionType:{connectionType}, driverVersion:{driverVersion}].]");
       
        if(!await _repository.Record<Consumer>().AnyAsync(x=>x.Id == consumerId, _cts.Token))
            throw CommonException.NotFound;
        
        if(!await _repository.Record<Instrument>().AnyAsync(x=>x.Id == instrumentId, _cts.Token))
            throw CommonException.NotFound;

        var consumerInstrument = new ConsumerInstrument()
        {
            ConsumerId = consumerId,
            InstrumentId = instrumentId,
            ConnectionType = connectionType,
            HostAddress = hostAddress,
            DriverVersion = driverVersion,
            Port = port,
        };
        
        if(1 < await _repository.AddAsync(consumerInstrument, _cts.Token))
            throw CommonException.FailedToSaveObject;
        
        _logger.LogInformation("Added consumer device.");
        
        consumerInstrument = await _repository
            .Record<ConsumerInstrument>()
            .Include(x => x.Instrument)
            .ThenInclude(x => x.Manufacturer)
            .Where(x=> x.Id == consumerInstrument.Id)
            .FirstOrDefaultAsync(_cts.Token) ?? throw CommonException.FailedToSaveObject;
        
        return ConsumerInstrumentEntry.Map(consumerInstrument);
    }

    public async Task AddEvent(Guid consumerInstrumentId, InstrumentEventType eventType, string? notes = null)
    {
        _logger.LogInformation($"Event invoke: Consumer instrumentId: '{consumerInstrumentId}'");
        if(false == await _repository
               .Record<ConsumerInstrument>()
               .AnyAsync(x => x.Id == consumerInstrumentId, _cts.Token))
            throw CommonException.NotFound;

        var @event = new InstrumentEvent(); 
        @event.EventType = eventType;
        @event.Notes = notes;
        
        if(1 < await _repository.AddAsync(@event, _cts.Token))
            throw CommonException.FailedToSaveObject;
        
        _logger.LogInformation($"Instrument event Id:'{@event.Id}' added.");
        
    }

    public Task<Dictionary<Guid, string?>> GetLookUp(int consumerId)
    {
        return _repository.Record<ConsumerInstrument>()
            .Where(x=> x.ConsumerId == consumerId)
            .Include(x => x.Consumer)
            .Include(x => x.Instrument)
            .ThenInclude(x => x.Manufacturer)
            .ToDictionaryAsync(key=> key.Id,value=>value.LookupName, _cts.Token);
    }
}