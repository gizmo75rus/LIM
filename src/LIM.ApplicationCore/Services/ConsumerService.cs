using LIM.ApplicationCore.BaseObjects;
using LIM.ApplicationCore.Models;
using LIM.ApplicationCore.Exceptions;
using LIM.ApplicationCore.Contracts;
using LIM.ApplicationCore.Dto;
using LIM.ApplicationCore.Enums;
using LIM.SharedKernel.BaseModels;
using LIM.SharedKernel.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LIM.ApplicationCore.Services;

public class ConsumerService : AbstractService, IConsumerService
{
    private readonly IRepository _repository;
    public ConsumerService(ILogger<ConsumerService> logger, IRepository repository)
    {
        _logger = logger;
        _repository = repository;
    }

    public async Task<IEnumerable<Lookup>> GetLookup() =>
        await _repository.Record<Consumer>()
            .Select(c => new Lookup(c.Id, c.Name))
            .ToListAsync(_cts.Token);
    
    public async Task<ConsumerDetail> Detail(int id) => 
        ConsumerDetail.Map(await _repository.Record<Consumer>()
                               .Include(x => x.Instruments)!
                               .ThenInclude(x => x.Instrument)
                               .ThenInclude(x => x!.Manufacturer)
                               .FirstOrDefaultAsync(x => x.Id == id, _cts.Token) 
                           ?? throw CommonException.NotFound);
    
    public async Task<ConsumerEntry> Create(string name, string lisVersion)
    {
        _logger.LogInformation($"Creating consumer entry for {name} with version {lisVersion}");
        
        if(string.IsNullOrEmpty(name) || string.IsNullOrEmpty(lisVersion))
            throw new ArgumentNullException(nameof(name));
        
        if(await _repository.Record<Consumer>().AnyAsync(x=>x.Name == name, _cts.Token))
            throw CommonException.RecordExist;

        var consumer = new Consumer()
        {
            Name = name,
            LisVersion = lisVersion
        };
        
        if(1 < await _repository.AddAsync(consumer, _cts.Token))
            throw CommonException.FailedToSaveObject;
        
        _logger.LogInformation($"Created consumer entry Id: {consumer.Id}");

        return ConsumerEntry.Map(consumer);
    }

    public async Task RemoveDevice(Guid consumerDeviceId)
    {
        _logger.LogInformation($"Removing consumer device: {consumerDeviceId}");
        var record = await 
            _repository.Record<ConsumerInstrument>()
                .Where(x=>x.Id == consumerDeviceId)
                .FirstOrDefaultAsync(_cts.Token) 
                     ?? throw CommonException.NotFound;
        
        if(await _repository
               .Record<InstrumentEvent>()
               .AnyAsync(x=>x.ConsumerInstrumentId == consumerDeviceId, _cts.Token)
           || await _repository
               .Record<InstrumentMessage>()
               .AnyAsync(x=>x.ConsumerInstrumentId == consumerDeviceId, _cts.Token))
            throw CommonException.ReferencesToObjectNotFree;

        if( 1 < await _repository.DeleteAsync(record, _cts.Token))
            throw CommonException.FailedToSaveObject;
        
        _logger.LogInformation($"Removed consumer device: {record.Id}");
    }

    public async Task Rename(int consumerId, string name)
    {
        if (consumerId == int.MaxValue)
            throw CommonException.ArgumentOutException;
        
        _logger.LogInformation($"Renaming consumer name for entry Id: {consumerId}, new value: {name}");
        
        var consumer = await _repository
            .Record<Consumer>()
            .Where(x => x.Id == consumerId)
            .FirstOrDefaultAsync(_cts.Token) 
                     ?? throw CommonException.NotFound;
        
        consumer.Name = name;
        if(1 < await _repository.UpdateAsync(consumer, _cts.Token))
            throw CommonException.FailedToSaveObject;
        
        _logger.LogInformation($"Renamed consumer entry Id: {consumerId}");
    }

    public async Task Delete(int id)
    {
        if (id == int.MaxValue)
            throw CommonException.ArgumentOutException;
        
        _logger.LogInformation($"Deleting consumer entry: {id}");
        
        var entry = await _repository
            .Record<Consumer>()
            .Include(x=>x.Instruments)!
            .FirstOrDefaultAsync(x => x.Id == id, _cts.Token) 
                    ?? throw CommonException.NotFound;

        if (entry.Instruments != null && entry.Instruments.Any())
            throw CommonException.ReferencesToObjectNotFree;
        
        if(1 < await _repository.DeleteAsync(entry, _cts.Token))
            throw CommonException.FailedToSaveObject;
        
        _logger.LogInformation($"Deleted consumer entry Id: {id}");
    }
}