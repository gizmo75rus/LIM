using LIM.ApplicationCore.Contracts;
using LIM.ApplicationCore.Dto;
using LIM.ApplicationCore.Exceptions;
using LIM.ApplicationCore.Models;
using LIM.SharedKernel.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LIM.ApplicationCore.Services;

public class ManufacturerService : IManufacturerService
{
    private const int CtsTimeout = 10000; 
    private readonly IRepository _repository;
    private readonly CancellationTokenSource _cts;

    public ManufacturerService(IRepository repository)
    {
        _repository = repository;
        _cts = new CancellationTokenSource(CtsTimeout);
    }
    public async Task<Dictionary<int, string?>> GetLookUp()
    {
        return await _repository.Record<Manufacturer>().ToDictionaryAsync(key => key.Id, value => value.Name, _cts.Token);
    }

    public async Task<ManufacturerEntry> Create(string name)
    {
        var manufacturer = new Manufacturer { Name = name };
        var added = await _repository.AddAsync(manufacturer, _cts.Token);
        
        if(added < 1)
            throw CommonException.FailedToSaveObject;
        
        var entry = ManufacturerEntry.Map(manufacturer);
        return entry;
    }

    public async Task Rename(int id, string newName)
    {
        var entry = await _repository
            .Record<Manufacturer>()
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync() 
                    ?? throw new KeyNotFoundException();
        
        entry.Name = newName;
        await _repository.UpdateAsync(entry, _cts.Token);
    }

    public async Task Delete(int id)
    {
        var entry = await _repository
            .Record<Manufacturer>()
            .Include(x=>x.Devices)
            .FirstOrDefaultAsync(x=>x.Id==id, _cts.Token) ?? throw CommonException.NotFound;
        
        if(entry.Devices.Any())
            throw CommonException.ReferencesToObjectNotFree;
        
        await _repository.DeleteAsync(entry, _cts.Token);
    }

    public async Task<ManufacturerDetail> Detail(int id)
    {
        var manufacturer = await _repository
            .Record<Manufacturer>()
            .Include(x=>x.Devices)
            .FirstOrDefaultAsync(x => x.Id == id, _cts.Token) ?? throw CommonException.NotFound;
        
        return ManufacturerDetail.Map(manufacturer);
    }
}