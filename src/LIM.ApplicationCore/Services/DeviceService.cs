using LIM.ApplicationCore.Models;
using LIM.ApplicationCore.Enums;
using LIM.ApplicationCore.Exceptions;
using LIM.ApplicationCore.Contracts;
using LIM.ApplicationCore.Dto;
using LIM.SharedKernel.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LIM.ApplicationCore.Services;

public class DeviceService : IDeviceService
{
    private readonly IRepository _repository;
    private readonly ILogger _logger;
    private readonly CancellationTokenSource _cts;

    public DeviceService(IRepository repository, ILogger<DeviceService> logger)
    {
        _repository = repository;
        _logger = logger;
        _cts = new CancellationTokenSource();
    }

    public async Task<DeviceEntry> Detail(int deviceId) => 
       DeviceEntry.Map(await _repository
            .Record<Device>()
            .Include(x=>x.Manufacturer)
            .FirstOrDefaultAsync(x =>x.Id == deviceId, _cts.Token)
        ?? throw CommonException.NotFound);

    public async Task<Dictionary<int, string>> GetLookUp() => 
        await _repository
        .Record<Device>()
        .Include(x=>x.Manufacturer)
        .ToDictionaryAsync(key => key.Id, value => value.LookupName, _cts.Token);

    public async Task<DeviceEntry> Create(string manufacturerName, string model, ProtocolType protocol)
    {
        var manufacturer = await _repository.Record<Manufacturer>()
                               .FirstOrDefaultAsync(x => x.Name == manufacturerName, _cts.Token);
        if (manufacturer == null)
        {
            manufacturer = new Manufacturer(manufacturerName);
            await _repository.AddAsync(manufacturer, _cts.Token);
        }

        var device = new Device()
        {
            ManufacturerId = manufacturer.Id,
            ProtocolType = protocol,
            Model = model
        };
        await _repository.AddAsync(device, _cts.Token);
        device.Manufacturer = manufacturer;
        return DeviceEntry.Map(device);
    }

    public async Task Modify(int deviceId, int manufacturerId, string model, ProtocolType protocol)
    {

        var device = await _repository.Record<Device>().FirstOrDefaultAsync(x => x.Id == deviceId);
        var manufacturer = await _repository.Record<Manufacturer>()
            .FirstOrDefaultAsync(x => x.Id == deviceId);
        
        if (device == null || manufacturer == null)
            throw CommonException.NotFound;

        device.Model = model;
        device.ProtocolType = protocol;
        device.ManufacturerId = manufacturer.Id;

        int modified = await _repository.UpdateAsync(device, _cts.Token);

        if (modified < 1)
            throw CommonException.FailedToSaveObject;
    }

    public async Task Delete(int deviceId)
    {
        var device = await _repository.Record<Device>()
                         .Where(x => x.Id == deviceId)
                         .FirstOrDefaultAsync(_cts.Token) 
                     ?? throw CommonException.NotFound;
                    
        
        if (await _repository
                .Record<ConsumerDevice>()
                .AnyAsync(x => x.DeviceId == deviceId, _cts.Token))
            throw CommonException.ReferencesToObjectNotFree;
        
        
        await _repository.DeleteAsync(device, _cts.Token);
    }
}