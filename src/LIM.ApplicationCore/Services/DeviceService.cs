using LIM.ApplicationCore.BaseObjects;
using LIM.ApplicationCore.Models;
using LIM.ApplicationCore.Enums;
using LIM.ApplicationCore.Exceptions;
using LIM.ApplicationCore.Contracts;
using LIM.ApplicationCore.Dto;
using LIM.SharedKernel.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LIM.ApplicationCore.Services;

public class DeviceService : AbstractService, IDeviceService
{
    private readonly IRepository _repository;
    public DeviceService(ILogger<DeviceService> logger, IRepository repository)
    {
        _logger = logger;
        _repository = repository;
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
        _logger.LogInformation("Creating new device");
        var manufacturer = await _repository.Record<Manufacturer>()
                               .FirstOrDefaultAsync(x => x.Name == manufacturerName, _cts.Token);
        if (manufacturer == null)
        {
            manufacturer = new Manufacturer(manufacturerName);
            await _repository.AddAsync(manufacturer, _cts.Token);

            _logger.LogInformation($"Manufacturer: {manufacturerName} has been created");
        }

        var device = new Device()
        {
            ManufacturerId = manufacturer.Id,
            ProtocolType = protocol,
            Model = model
        };
        int added = await _repository.AddAsync(device, _cts.Token);

        if (added < 1)
            throw CommonException.FailedToSaveObject;
        
        _logger.LogInformation($"Device: '{manufacturerName}' model: '{model}' has been added");
        
        device.Manufacturer = manufacturer;
        
        return DeviceEntry.Map(device);
    }

    public async Task Modify(int deviceId, int manufacturerId, string model, ProtocolType protocol)
    {
        _logger.LogInformation($"Modifying device id:{manufacturerId} manufacturerId: {manufacturerId} model:{model}");

        var device = await _repository.Record<Device>().FirstOrDefaultAsync(x => x.Id == deviceId);
        var manufacturer = await _repository.Record<Manufacturer>()
            .FirstOrDefaultAsync(x => x.Id == deviceId);

        if (device == null || manufacturer == null)
        {
            _logger.LogInformation("Device or manufacturer not found");
            throw CommonException.NotFound;
        }

        device.Model = model;
        device.ProtocolType = protocol;
        device.ManufacturerId = manufacturer.Id;

        int modified = await _repository.UpdateAsync(device, _cts.Token);

        if (modified < 1)
            throw CommonException.FailedToSaveObject;
        
        _logger.LogInformation("Device has been updated");
    }

    public async Task Delete(int deviceId)
    {
        _logger.LogInformation($"Deleting device id:{deviceId}");
        var device = await _repository.Record<Device>()
                         .Where(x => x.Id == deviceId)
                         .FirstOrDefaultAsync(_cts.Token) 
                     ?? throw CommonException.NotFound;
                    
        
        if (await _repository
                .Record<ConsumerDevice>()
                .AnyAsync(x => x.DeviceId == deviceId, _cts.Token))
            throw CommonException.ReferencesToObjectNotFree;
        
        
        await _repository.DeleteAsync(device, _cts.Token);
        _logger.LogInformation("Device has been deleted");
    }
}