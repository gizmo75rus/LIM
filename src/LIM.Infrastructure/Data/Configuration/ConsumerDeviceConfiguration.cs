using LIM.ApplicationCore.Entities;
using LIM.Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LIM.Infrastructure.Data.Configuration;

public class ConsumerDeviceConfiguration : IEntityTypeConfiguration<ConsumerDevice>
{
    public void Configure(EntityTypeBuilder<ConsumerDevice> builder)
    {
        builder.HasKey(x => x.Id);

        builder.IsJournaled();
        
        builder.HasOne(x => x.Consumer)
            .WithMany(x => x.ConsumerDevices)
            .HasForeignKey(x => x.ConsumerId)
            .OnDelete(DeleteBehavior.SetNull);
        
        builder.HasOne(x => x.Device)
            .WithMany(x => x.ConsumerDevices)
            .HasForeignKey(x => x.DeviceId);
        
        builder.Property(x => x.DriverVersion)
            .IsString(15);

        builder.Property(x => x.SerialNumber)
            .IsString(50);
    }
}