using LIM.ApplicationCore.BaseObjects;
using LIM.ApplicationCore.Models;
using LIM.ApplicationCore.Enums;
using LIM.ApplicationCore.Exceptions;
using LIM.ApplicationCore.Contracts;
using LIM.ApplicationCore.Dto;
using LIM.SharedKernel.BaseModels;
using LIM.SharedKernel.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LIM.ApplicationCore.Services;

public class InstrumentService : AbstractService, IInstrumentService
{
    private readonly IRepository _repository;
    public InstrumentService(ILogger<InstrumentService> logger, IRepository repository)
    {
        _logger = logger;
        _repository = repository;
    }

    public async Task<InstrumentEntry> Detail(int deviceId) => 
       InstrumentEntry.Map(await _repository
            .Record<Instrument>()
            .Include(x=>x.Manufacturer)
            .FirstOrDefaultAsync(x =>x.Id == deviceId, _cts.Token)
        ?? throw CommonException.NotFound);

    public async Task<IEnumerable<Lookup>> GetLookUp() => 
        await _repository
            .Record<Instrument>()
            .Include(x=>x.Manufacturer)
            .Select(x=> new Lookup(x.Id, x.LookupName))
            .ToListAsync(_cts.Token);

    public async Task<InstrumentEntry> Create(string manufacturerName, string model, ProtocolType protocol)
    {
        _logger.LogInformation("Creating new device");
        var manufacturer = await _repository.Record<Manufacturer>()
                               .FirstOrDefaultAsync(x => x.Name == manufacturerName, _cts.Token);
        if (manufacturer == null)
        {
            manufacturer = new Manufacturer(manufacturerName);
            await _repository.AddAsync(manufacturer, _cts.Token);

            _logger.LogInformation($"Manufacturer: {manufacturerName} has been created");
        }

        var instrument = new Instrument()
        {
            ManufacturerId = manufacturer.Id,
            ProtocolType = protocol,
            Model = model
        };

        if (1 < await _repository.AddAsync(instrument, _cts.Token))
            throw CommonException.FailedToSaveObject;
        
        _logger.LogInformation($"Device: '{manufacturerName}' model: '{model}' has been added");
        
        instrument.Manufacturer = manufacturer;
        
        return InstrumentEntry.Map(instrument);
    }

    public async Task Modify(int deviceId, int manufacturerId, string model, ProtocolType protocol)
    {
        _logger.LogInformation($"Modifying device id:{manufacturerId} manufacturerId: {manufacturerId} model:{model}");

        var instrument = await _repository.Record<Instrument>().FirstOrDefaultAsync(x => x.Id == deviceId);
        var manufacturer = await _repository.Record<Manufacturer>()
            .FirstOrDefaultAsync(x => x.Id == deviceId);

        if (instrument == null || manufacturer == null)
        {
            _logger.LogInformation("Device or manufacturer not found");
            throw CommonException.NotFound;
        }

        instrument.Model = model;
        instrument.ProtocolType = protocol;
        instrument.ManufacturerId = manufacturer.Id;

        int modified = await _repository.UpdateAsync(instrument, _cts.Token);

        if (modified < 1)
            throw CommonException.FailedToSaveObject;
        
        _logger.LogInformation("Device has been updated");
    }

    public async Task Delete(int deviceId)
    {
        _logger.LogInformation($"Deleting device id:{deviceId}");
        var instrument = await _repository.Record<Instrument>()
                         .Where(x => x.Id == deviceId)
                         .FirstOrDefaultAsync(_cts.Token) 
                     ?? throw CommonException.NotFound;
                    
        
        if (await _repository
                .Record<ConsumerInstrument>()
                .AnyAsync(x => x.InstrumentId == deviceId, _cts.Token))
            throw CommonException.ReferencesToObjectNotFree;
        
        
        await _repository.DeleteAsync(instrument, _cts.Token);
        _logger.LogInformation("Device has been deleted");
    }
}