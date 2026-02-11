using Asp.Versioning;
using LIM.ApplicationCore.Contracts;
using LIM.ApplicationCore.Dto;
using LIM.SharedKernel.BaseModels;
using LIM.WebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;

namespace LIM.WebApp.Controllers.v1;

[ApiVersion("1.0")]
public class InstrumentsController : BaseController
{
    private readonly IInstrumentService _service;

    public InstrumentsController(IInstrumentService service)
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
    [ProducesResponseType(typeof(AppResponce<InstrumentEntry>), 200)]
    public async Task<IActionResult> Detail([FromRoute]int id)
    {
        var entry = await _service.Detail(id);
        return Ok(AppResponce<InstrumentEntry>.Ok(entry));
    }

    [HttpPost]
    [ProducesResponseType(typeof(AppResponce<InstrumentEntry>),200)]
    public async Task<IActionResult> Create([FromBody] CreateInstrumentRequest request)
    {
        var result = await _service.Create(request.ManufacturerName, request.Model, request.ProtocolType); 
        return Ok(AppResponce<InstrumentEntry>.Ok(result));
    }

    [HttpPut("{instrumentId}")]
    [ProducesResponseType(typeof(AppResponce), 200)]
    public async Task<IActionResult> Update([FromRoute]int instrumentId, ModifyInstrumentRequest request)
    {
        await _service.Modify(instrumentId, request.ManufacturerId, request.Model, request.Protocol);
        return Ok(AppResponce.Ok());
    }

    [HttpDelete("{instrumentId}")]
    [ProducesResponseType(typeof(AppResponce), 200)]
    public async Task<IActionResult> Delete([FromRoute] int instrumentId)
    {
        await _service.Delete(instrumentId);
        return Ok(AppResponce.Ok());
    }

}