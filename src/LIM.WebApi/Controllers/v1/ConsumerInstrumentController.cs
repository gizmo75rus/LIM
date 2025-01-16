using Asp.Versioning;
using LIM.ApplicationCore.Contracts;
using LIM.ApplicationCore.Dto;
using LIM.WebApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace LIM.WebApp.Controllers.v1;

[ApiVersion("1.0")]
public class ConsumerInstrumentController: BaseController
{
    private readonly IConsumerInstrumentService _service;

    public ConsumerInstrumentController(IConsumerInstrumentService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(AppResponce<Dictionary<Guid, string?>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Lookup(int consumerId)
    {
        var result = await _service.GetLookUp(consumerId);
        return Ok(AppResponce<Dictionary<Guid,string?>>.Ok(result));
    }
    
    [HttpPost]
    [ProducesResponseType(typeof(AppResponce<ConsumerInstrumentEntry>), 200)]
    public async Task<IActionResult> AddInstrument([FromBody] ConsumerAddInstrumentRequest request)
    {
        if(request == null)
            throw new ArgumentNullException(nameof(request));

        var result = await _service.AddInstrument(request.ConsumerId, 
            request.InstrumentId, 
            request.HostAddress, 
            request.Port,
            request.ConnectionType, 
            request.DriverVersion);
        
        return Ok(AppResponce<ConsumerInstrumentEntry>.Ok(result));
    }
}