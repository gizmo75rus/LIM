using Asp.Versioning;
using LIM.ApplicationCore.Contracts;
using LIM.ApplicationCore.Dto;
using LIM.WebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;

namespace LIM.WebApp.Controllers.v1;

[ApiVersion("1.0")]
public class DevicesController : BaseController
{
    private readonly IDeviceService _service;

    public DevicesController(IDeviceService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(AppResponce<Dictionary<int,string>>),200)]
    public async Task<IActionResult> LookUp()
    {
        return Ok(AppResponce<Dictionary<int, string?>>.Ok(await _service.GetLookUp()));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AppResponce<DeviceEntry>), 200)]
    public async Task<IActionResult> Detail([FromRoute]int id)
    {
        var entry = await _service.Detail(id);
        return Ok(AppResponce<DeviceEntry>.Ok(entry));
    }

    [HttpPost]
    [ProducesResponseType(typeof(AppResponce<DeviceEntry>),200)]
    public async Task<IActionResult> Create([FromBody] CreateDeviceRequest request)
    {
        var result = await _service.Create(request.ManufacturerName, request.Model, request.ProtocolType); 
        return Ok(AppResponce<DeviceEntry>.Ok(result));
    }

    [HttpPut("{deviceId}")]
    [ProducesResponseType(typeof(AppResponce), 200)]
    public async Task<IActionResult> Update([FromRoute]int deviceId, ModifyDeviceRequest request)
    {
        await _service.Modify(deviceId, request.ManufacturerId, request.Model, request.Protocol);
        return Ok(AppResponce.Ok());
    }

    [HttpDelete("{deviceId}")]
    [ProducesResponseType(typeof(AppResponce), 200)]
    public async Task<IActionResult> Delete([FromRoute] int deviceId)
    {
        await _service.Delete(deviceId);
        return Ok(AppResponce.Ok());
    }

}