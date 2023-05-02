using LIM.ApplicationCore.Interfaces;
using LIM.WebApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace LIM.WebApp.Controllers.v1;

public class ConsumersController : BaseController
{
    private readonly IConsumerService _service;

    public ConsumersController(IConsumerService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(AppResponce<Dictionary<int,string>>),200)]
    public async Task<IActionResult> LookUp()
    {
        var data = await _service.GeLookup();
        var responce = new AppResponce<Dictionary<int, string>>(data);
        return Ok(responce);
    }
}