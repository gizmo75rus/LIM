using LIM.ApplicationCore.Entities;
using LIM.ApplicationCore.Exceptions;
using LIM.ApplicationCore.Interfaces;
using LIM.SharedKernel.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LIM.ApplicationCore.Services;

public class ConsumerService : IService,IConsumerService
{
    private readonly IRepository _repository;

    public ConsumerService(IRepository repository)
    {
        _repository = repository;
    }

    public async Task<Consumer> Create(string name,string lisVersion)
    {
        if(!await _repository.Record<Consumer>(noTracking:true).AnyAsync(x=>x.Name == name))
            throw CommonException.RecordExist;

        var consumer = new Consumer()
        {
            Name = name,
            LisVersion = lisVersion
        };
        
        _repository.Insert(consumer);

        await _repository.SaveChangeAsync();

        return consumer;
    }

    public async Task<Dictionary<int, string?>> GeLookup()
    {
        return await _repository.Record<Consumer>(noTracking: true)
            .ToDictionaryAsync(key => key.Id, value => value.Name);
    }

    public async Task<Consumer> Get(int Id)
    {
        var value = await _repository.Record<Consumer>(noTracking: true).FirstOrDefaultAsync(x => x.Id == Id);
        if(value == null)
            throw CommonException.NotFound;
        return value;
    }

    public async Task AddDevice(Consumer consumer, Device device)
    {
        var consumerDevice = new ConsumerDevice()
        {
            ConsumerId = consumer.Id,
            DeviceId = device.Id
        };
        
        _repository.Insert(consumer);
        await _repository.SaveChangeAsync();
    }

    public async Task RemoveDevice(Consumer consumer, Device device)
    {
        var consumerDevice = await _repository.Record<ConsumerDevice>(noTracking: false)
            .FirstOrDefaultAsync(x => x.ConsumerId == consumer.Id && x.DeviceId == device.Id);
        if (consumerDevice == null)
            throw CommonException.NotFound;

        consumerDevice.Removed = true;

        _repository.Update(consumerDevice);
        await _repository.SaveChangeAsync();
    }
}