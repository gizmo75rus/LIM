using LIM.ApplicationCore.Contracts;
using LIM.ApplicationCore.Dto;
using LIM.ApplicationCore.Enums;
using LIM.ApplicationCore.Exceptions;
using LIM.ApplicationCore.Models;
using LIM.SharedKernel.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LIM.ApplicationCore.Services;

public class ConsumerDeviceService : IConsumerDeviceService
{
    private readonly ILogger<ConsumerDeviceService> _logger;
    private readonly IRepository _repository;
    private const int CtsLiveTimeMs = 10000; // 10 000ms = 10s
    private readonly CancellationTokenSource _cts;
    
    public ConsumerDeviceService(IRepository repository, ILogger<ConsumerDeviceService> logger)
    {
        this._repository = repository;
        this._logger = logger;
        _cts = new CancellationTokenSource(CtsLiveTimeMs);
    }
    
    public async Task<ConsumerDeviceEntry> AddDevice(int consumerId, int deviceId, string hostAddress, short port, ConnectionType connectionType, string driverVersion)
    {
       
        if(!await _repository.Record<Consumer>().AnyAsync(x=>x.Id == consumerId, _cts.Token))
            throw CommonException.NotFound;
        
        if(!await _repository.Record<Device>().AnyAsync(x=>x.Id == deviceId, _cts.Token))
            throw CommonException.NotFound;

        var consumerDevice = new ConsumerDevice()
        {
            ConsumerId = consumerId,
            DeviceId = deviceId,
            ConnectionType = connectionType,
            HostAddress = hostAddress,
            DriverVersion = driverVersion,
            Port = port,
        };
        
        var added = await _repository.AddAsync(consumerDevice, _cts.Token);
        
        if(added < 1)
            throw CommonException.FailedToSaveObject;
        
        consumerDevice = await _repository
            .Record<ConsumerDevice>()
            .Include(x => x.Device)
            .ThenInclude(x => x.Manufacturer)
            .Where(x=> x.Id == consumerDevice.Id)
            .FirstOrDefaultAsync(_cts.Token) ?? throw CommonException.FailedToSaveObject;

        return ConsumerDeviceEntry.Map(consumerDevice);
    }

    public Task<Dictionary<Guid, string?>> GetLookUp(int consumerId)
    {
        return _repository.Record<ConsumerDevice>()
            .Where(x=> x.ConsumerId == consumerId)
            .Include(x => x.Device)
            .ThenInclude(x => x.Manufacturer)
            .ToDictionaryAsync(key=> key.Id,value=>value.LookupName, _cts.Token);
    }
}