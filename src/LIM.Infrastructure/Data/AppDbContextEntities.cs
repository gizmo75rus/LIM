using LIM.ApplicationCore.Models;
using Microsoft.EntityFrameworkCore;

namespace LIM.Infrastructure.Data;

public partial class AppDbContext
{
    public DbSet<Consumer> Consumers => Set<Consumer>();
    public DbSet<ConsumerInstrument> ConsumerInstruments => Set<ConsumerInstrument>();
    public DbSet<Instrument> Instruments => Set<Instrument>();
    public DbSet<InstrumentEvent> InstrumentEvents => Set<InstrumentEvent>();
    public DbSet<Manufacturer> Manufacturers => Set<Manufacturer>();
    public DbSet<Research> Researches => Set<Research>();
}