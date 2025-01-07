using LIM.ApplicationCore.BaseObjects;
using LIM.ApplicationCore.Contracts;
using LIM.ApplicationCore.Dto;
using LIM.ApplicationCore.Enums;
using LIM.ApplicationCore.Exceptions;
using LIM.ApplicationCore.Models;
using LIM.SharedKernel.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LIM.ApplicationCore.Services;

public class ConsumerDeviceService : AbstractService, IConsumerDeviceService
{
    private readonly IRepository _repository;
    
    public ConsumerDeviceService(ILogger<ConsumerDeviceService> logger, IRepository repository)
    {
        this._logger = logger;
        this._repository = repository;
    }
    
    public async Task<ConsumerDeviceEntry> AddDevice(int consumerId, int deviceId, string hostAddress, short port, ConnectionType connectionType, string driverVersion)
    {
        _logger.LogInformation($"Add device to consumer [consumerId:{consumerId}, deviceId:{deviceId}, hostAddress:{hostAddress}, port:{port}, connectionType:{connectionType}, driverVersion:{driverVersion}].]");
       
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
        
        if(1 < await _repository.AddAsync(consumerDevice, _cts.Token))
            throw CommonException.FailedToSaveObject;
        
        _logger.LogInformation("Added consumer device.");
        
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