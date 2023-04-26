using System.Net.Sockets;
using LIM.ApplicationCore.Entities;
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


    public async Task<Device> CreateDevice(string manufacturerName, string model, ProtocolType protocol)
    {
        var manufacturer = await _repository.Record<Manufacturer>(noTracking:false).FirstOrDefaultAsync(x => x.Name == manufacturerName);
        if (manufacturer == null)
        {
            manufacturer = new Manufacturer()
            {
                Name = manufacturerName
            };
            _repository.Insert(manufacturer);
        }

        var device = await _repository.Record<Device>(noTracking:false)
            .FirstOrDefaultAsync(x => x.Manufacturer.Name == manufacturerName && x.Model == model);

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
    
}