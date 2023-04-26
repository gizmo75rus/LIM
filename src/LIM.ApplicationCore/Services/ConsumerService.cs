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

    public async Task<Consumer> CreateConsumer(string name,string lisVersion)
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
}