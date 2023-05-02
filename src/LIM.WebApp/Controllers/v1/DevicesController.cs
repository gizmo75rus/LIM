using Asp.Versioning;
using LIM.ApplicationCore.Entities;
using LIM.ApplicationCore.Interfaces;
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
        var responce = new AppResponce<Dictionary<int, string>>(await _service.GetLookUp());
        return Ok(responce);
    }

    [HttpPost]
    [ProducesResponseType(typeof(AppResponce<int>),200)]
    public async Task<IActionResult> Create([FromBody] DeviceRequestModel body)
    {
        var result = await _service.Create(body.Manufacturer, body.Model, body.ProtocolType);
        var responce = new AppResponce<int>(result.Id);
        return Ok(responce);
    }

    [HttpPost]
    [ProducesResponseType(typeof(AppResponce), 200)]
    public async Task<IActionResult> Update(int id, DeviceRequestModel body)
    {
        await _service.Update(id, body.Manufacturer, body.Model, body.ProtocolType);
        var responce = new AppResponce();
        return Ok(responce);
    }

}