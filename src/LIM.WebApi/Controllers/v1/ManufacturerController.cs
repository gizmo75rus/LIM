using Asp.Versioning;
using LIM.ApplicationCore.Contracts;
using LIM.ApplicationCore.Dto;
using LIM.ApplicationCore.Services;
using LIM.SharedKernel.BaseModels;
using LIM.WebApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace LIM.WebApp.Controllers.v1;

[ApiVersion("1.0")]
public class ManufacturerController: BaseController
{
    private readonly IManufacturerService _service;

    public ManufacturerController(IManufacturerService service)
    {
        _service = service;
    }
    
    [HttpGet]
    [ProducesResponseType(typeof(AppResponce<IEnumerable<Lookup>>),200)]
    public async Task<IActionResult> LookUp()
    {
        return Ok(AppResponce<IEnumerable<Lookup>>.Ok(await _service.GetLookup()));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AppResponce<ManufacturerDetail>), 200)]
    public async Task<IActionResult> Detail([FromRoute]int id)
    {
        ManufacturerDetail entry = await _service.Detail(id);
        return Ok(AppResponce<ManufacturerDetail>.Ok(entry));
    }

    [HttpPost]
    [ProducesResponseType(typeof(AppResponce<ManufacturerEntry>), 200)]
    public async Task<IActionResult> Create(string name)
    {
        var entry = await _service.Create(name);
        return Ok(AppResponce<ManufacturerEntry>.Ok(entry));
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(AppResponce), 200)]
    public async Task<IActionResult> Rename([FromRoute]int id, string newName)
    {
        await _service.Rename(id, newName);
        return Ok(AppResponce.Ok());
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(AppResponce), 200)]
    public async Task<IActionResult> Delete([FromRoute]int id)
    {
        await _service.Delete(id);
        return Ok(AppResponce.Ok());
    }
}