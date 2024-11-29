using LIM.ApplicationCore.Models;
using LIM.ApplicationCore.Exceptions;
using LIM.ApplicationCore.Contracts;
using LIM.ApplicationCore.Dto;
using LIM.ApplicationCore.Enums;
using LIM.SharedKernel.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LIM.ApplicationCore.Services;

public class ConsumerService : IConsumerService
{
    private const int CtsLiveTimeMs = 10000; // 10 000ms = 10s
    private readonly IRepository _repository;
    private readonly CancellationTokenSource _cts;

    public ConsumerService(IRepository repository)
    {
        _repository = repository;
        _cts = new CancellationTokenSource(CtsLiveTimeMs);
    }
    
    public async Task<Dictionary<int, string?>> GetLookUp() =>
        await _repository.Record<Consumer>()
        .ToDictionaryAsync(key => key.Id, value => value.Name ?? default(string), _cts.Token);
    
    public async Task<ConsumerDetail> Detail(int id) => 
        ConsumerDetail.Map(await _repository.Record<Consumer>()
                               .Include(x => x.ConsumerDevices)!
                               .ThenInclude(x => x.Device)
                               .ThenInclude(x => x!.Manufacturer)
                               .FirstOrDefaultAsync(x => x.Id == id, _cts.Token) 
                           ?? throw CommonException.NotFound);
    
    public async Task<ConsumerEntry> Create(string name, string lisVersion)
    {
        if(string.IsNullOrEmpty(name) || string.IsNullOrEmpty(lisVersion))
            throw new ArgumentNullException(nameof(name));
        
        if(await _repository.Record<Consumer>().AnyAsync(x=>x.Name == name, _cts.Token))
            throw CommonException.RecordExist;

        var consumer = new Consumer()
        {
            Name = name,
            LisVersion = lisVersion
        };
        
        int added = await _repository.AddAsync(consumer, _cts.Token);

        if (added < 1)
            throw CommonException.FailedToSaveObject;

        return ConsumerEntry.Map(consumer);
    }

    public async Task RemoveDevice(Guid consumerDeviceId)
    {
        var record = await 
            _repository.Record<ConsumerDevice>()
                .Where(x=>x.Id == consumerDeviceId)
                .FirstOrDefaultAsync(_cts.Token) 
                     ?? throw CommonException.NotFound;
        
        if(record.DeviceEvents != null && record.DeviceEvents.Any())
            throw CommonException.ReferencesToObjectNotFree;
        
        await _repository.DeleteAsync(record, _cts.Token);
    }

    public async Task Rename(int consumerId, string name)
    {
        var consumer = await _repository
            .Record<Consumer>()
            .Where(x => x.Id == consumerId)
            .FirstOrDefaultAsync(_cts.Token) 
                     ?? throw CommonException.NotFound;
        
        consumer.Name = name;
        int modified = await _repository.UpdateAsync(consumer, _cts.Token);
        
        if(modified < 1)
            throw CommonException.FailedToSaveObject;
    }

    public async Task Delete(int id)
    {
        var entry = await _repository
            .Record<Consumer>()
            .Include(x=>x.ConsumerDevices)!
            .FirstOrDefaultAsync(x => x.Id == id, _cts.Token) 
                    ?? throw CommonException.NotFound;

        if (entry.ConsumerDevices != null && entry.ConsumerDevices.Any())
            throw CommonException.ReferencesToObjectNotFree;
        
        await _repository.DeleteAsync(entry, _cts.Token);
    }
}