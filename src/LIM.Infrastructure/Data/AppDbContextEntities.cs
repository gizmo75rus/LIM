using LIM.ApplicationCore.Models;
using Microsoft.EntityFrameworkCore;

namespace LIM.Infrastructure.Data;

public partial class AppDbContext
{
    public DbSet<Consumer> Consumers => Set<Consumer>();
    public DbSet<ConsumerDevice> ConsumerDevices => Set<ConsumerDevice>();
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<DeviceEvent> DeviceEvents => Set<DeviceEvent>();
    public DbSet<Manufacturer> Manufacturers => Set<Manufacturer>();
    public DbSet<Research> Researches => Set<Research>();
}