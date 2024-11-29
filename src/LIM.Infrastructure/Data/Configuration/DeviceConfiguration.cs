using LIM.ApplicationCore.Models;
using LIM.Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LIM.Infrastructure.Data.Configuration;

public class DeviceConfiguration : IEntityTypeConfiguration<Device>
{
    public void Configure(EntityTypeBuilder<Device> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Model)
            .IsString(100);
        
        builder.HasOne(x => x.Manufacturer)
            .WithMany(x => x.Devices)
            .HasForeignKey(x=>x.ManufacturerId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}