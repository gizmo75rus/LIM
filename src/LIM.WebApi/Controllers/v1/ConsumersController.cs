using Asp.Versioning;
using LIM.ApplicationCore.Contracts;
using LIM.ApplicationCore.Dto;
using LIM.SharedKernel.BaseModels;
using LIM.WebApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace LIM.WebApp.Controllers.v1;

[ApiVersion("1.0")]
public class ConsumersController : BaseController
{
    private const string LisVersion = "0.0.0.1";
    private readonly IConsumerService _consumerService;

    public ConsumersController(IConsumerService consumerService, IConsumerInstrumentService consumerInstrumentService)
    {
        _consumerService = consumerService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(AppResponce<IEnumerable<Lookup>>),200)]
    public async Task<IActionResult> LookUp()
    {
        return Ok(AppResponce<IEnumerable<Lookup>>.Ok(await _consumerService.GetLookup()));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AppResponce<ConsumerDetail>), 200)]
    public async Task<IActionResult> Detail([FromRoute]int id)
    {
        var result = await _consumerService.Detail(id);
        return Ok(AppResponce<ConsumerDetail>.Ok(result));
    }

    [HttpPost]
    [ProducesResponseType(typeof(AppResponce<ConsumerEntry>), 200)]
    public async Task<IActionResult> Create([FromBody]string name)
    {
        var entry = await _consumerService.Create(name, LisVersion);
        return Ok(AppResponce<ConsumerEntry>.Ok(entry));
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(AppResponce<ConsumerDetail>), 200)]
    public async Task<IActionResult> Rename([FromRoute]int id, string name)
    {
        await _consumerService.Rename(id, name);
        return Ok(AppResponce<ConsumerDetail>.Ok(await _consumerService.Detail(id)));
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(AppResponce), 200)]
    public async Task<IActionResult> Delete([FromRoute]int id)
    {
        await _consumerService.Delete(id);
        return Ok(AppResponce.Ok());
    }

}