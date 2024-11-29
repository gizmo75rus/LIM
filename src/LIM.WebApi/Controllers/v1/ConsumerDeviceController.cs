using Asp.Versioning;
using LIM.ApplicationCore.Contracts;
using LIM.ApplicationCore.Dto;
using LIM.WebApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace LIM.WebApp.Controllers.v1;

[ApiVersion("1.0")]
public class ConsumerDeviceController: BaseController
{
    private readonly IConsumerDeviceService _consumerDeviceService;

    public ConsumerDeviceController(IConsumerDeviceService consumerDeviceService)
    {
        _consumerDeviceService = consumerDeviceService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(AppResponce<Dictionary<Guid, string?>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Lookup(int consumerId)
    {
        var result = await _consumerDeviceService.GetLookUp(consumerId);
        return Ok(AppResponce<Dictionary<Guid,string?>>.Ok(result));
    }
    
    [HttpPost]
    [ProducesResponseType(typeof(AppResponce<ConsumerDeviceEntry>), 200)]
    public async Task<IActionResult> AddDevice([FromBody] AddDeviceToConsumerRequest request)
    {
        if(request == null)
            throw new ArgumentNullException(nameof(request));

        var result = await _consumerDeviceService.AddDevice(request.ConsumerId, 
            request.DeviceId, 
            request.HostAddress, 
            request.Port,
            request.ConnectionType, 
            request.DriverVersion);
        
        return Ok(AppResponce<ConsumerDeviceEntry>.Ok(result));
    }
}