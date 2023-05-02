using LIM.ApplicationCore.Entities;
using LIM.ApplicationCore.Enums;
using LIM.ApplicationCore.Exceptions;
using LIM.ApplicationCore.Interfaces;
using LIM.SharedKernel.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LIM.ApplicationCore.Services;

public class DeviceService : IService, IDeviceService
{
    private readonly IRepository _repository;

    public DeviceService(IRepository repository)
    {
        _repository = repository;
    }

    public async Task<Device> Get(string manufacturer, string model)
    {
        var device = await _repository.Record<Device>(noTracking: true)
            .FirstOrDefaultAsync(x => x.Manufacturer != null && x.Manufacturer.Name == manufacturer && x.Model == model);

        if (device == null)
            throw CommonException.NotFound;

        return device;
    }

    public async Task<Dictionary<int, string>> GetLookUp()
    {
        return await _repository.Record<Device>(noTracking: true)
            .Include(x=>x.Manufacturer)
            .ToDictionaryAsync(key => key.Id, value => value.Manufacturer?.Name + ' ' + value.Model);
    }

    public async Task<Device> Create(string manufacturerName, string model, ProtocolType protocol)
    {
        var manufacturer = await _repository.Record<Manufacturer>(noTracking: true)
            .FirstOrDefaultAsync(x => x.Name == manufacturerName) ?? new Manufacturer { Name = manufacturerName };

        var device = await _repository.Record<Device>(noTracking:false)
            .FirstOrDefaultAsync(x => x.Manufacturer != null && x.Manufacturer.Name == manufacturerName && x.Model == model);

        if (device == null)
        {
            device = new Device()
            {
                Manufacturer = manufacturer,
                ProtocolType = protocol,
                Model = model
            };
            _repository.Insert(device);
        }

        await _repository.SaveChangeAsync();

        return device;
    }

    public async Task Update(int deviceId, string manufacturerName, string model, ProtocolType protocol)
    {
        
        var current = await _repository.Record<Device>(noTracking: false).FirstOrDefaultAsync(x => x.Id == deviceId);
        if(current == null)
            throw CommonException.NotFound;
        
        var manufacturer = await _repository.Record<Manufacturer>(noTracking: true)
            .FirstOrDefaultAsync(x => x.Name == manufacturerName) ?? new Manufacturer { Name = manufacturerName };
        
        current.Model = model;
        current.ProtocolType = protocol;
        current.Manufacturer = manufacturer;
        
        _repository.Update(current);
        await _repository.SaveChangeAsync();
    }
}