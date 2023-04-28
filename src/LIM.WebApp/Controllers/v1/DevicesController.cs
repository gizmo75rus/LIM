using Asp.Versioning;
using LIM.ApplicationCore.Interfaces;
using LIM.WebApp.Models;
using Microsoft.AspNetCore.Mvc;

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
    [ProducesResponseType(typeof(AppResponce<Dictionary<int,string?>>),200)]
    public async Task<IActionResult> Get()
    {
        var responce = new AppResponce<Dictionary<int, string?>>(await _service.GetLookUp());
        return Ok(responce);
    }
}