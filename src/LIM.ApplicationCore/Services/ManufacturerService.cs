using LIM.ApplicationCore.BaseObjects;
using LIM.ApplicationCore.Contracts;
using LIM.ApplicationCore.Dto;
using LIM.ApplicationCore.Exceptions;
using LIM.ApplicationCore.Models;
using LIM.SharedKernel.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LIM.ApplicationCore.Services;

public class ManufacturerService : AbstractService, IManufacturerService
{
    private readonly IRepository _repository;
    
    public ManufacturerService(ILogger<ManufacturerService> logger,  IRepository repository)
    {
        _logger = logger;
        _repository = repository;
    }
    
    public async Task<Dictionary<int, string?>> GetLookUp()
    {
        return await _repository
            .Record<Manufacturer>()
            .ToDictionaryAsync(key => key.Id, value => value.Name, _cts.Token);
    }

    public async Task<ManufacturerEntry> Create(string name)
    {
        _logger.LogInformation($"Creating manufacturer entry for {name}");
        var manufacturer = new Manufacturer { Name = name };
        
        if( 1 < await _repository.AddAsync(manufacturer, _cts.Token))
            throw CommonException.FailedToSaveObject;
        
        _logger.LogInformation($"Created manufacturer entry Id: {manufacturer.Id}");
        
        return ManufacturerEntry.Map(manufacturer);
    }

    public async Task Rename(int id, string newName)
    {
        _logger.LogInformation($"Renaming manufacturer entry for {newName}");
        
        var entry = await _repository
            .Record<Manufacturer>()
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync() 
                    ?? throw CommonException.NotFound;
        
        entry.Name = newName;
        
        if(1 < await _repository.UpdateAsync(entry, _cts.Token))
            throw CommonException.FailedToSaveObject;

        _logger.LogInformation($"Updated manufacturer entry Id: {entry.Id}");
    }

    public async Task Delete(int id)
    {
        _logger.LogInformation($"Deleting manufacturer entry for {id}");
        
        var entry = await _repository
            .Record<Manufacturer>()
            .Include(x=>x.Instruments)
            .FirstOrDefaultAsync(x=>x.Id==id, _cts.Token) 
                    ?? throw CommonException.NotFound;
        
        if(entry.Instruments.Any())
            throw CommonException.ReferencesToObjectNotFree;
        
        if( 1 < await _repository.DeleteAsync(entry, _cts.Token))
            throw CommonException.FailedToSaveObject;
        
        _logger.LogInformation($"Deleted manufacturer entry Id: {entry.Id}");
    }

    public async Task<ManufacturerDetail> Detail(int id)
    {
        var manufacturer = await _repository
            .Record<Manufacturer>()
            .Include(x=>x.Instruments)
            .FirstOrDefaultAsync(x => x.Id == id, _cts.Token) 
                           ?? throw CommonException.NotFound;
        
        return ManufacturerDetail.Map(manufacturer);
    }
}