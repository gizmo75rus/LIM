using LIM.ApplicationCore.Models;
using LIM.Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LIM.Infrastructure.Data.Configuration;

public class ConsumerDeviceConfiguration : IEntityTypeConfiguration<ConsumerInstrument>
{
    public void Configure(EntityTypeBuilder<ConsumerInstrument> builder)
    {
        builder.HasKey(x => x.Id);

        builder.IsJournaled();
        
        builder.HasOne(x => x.Consumer)
            .WithMany(x => x.Instruments)
            .HasForeignKey(x => x.ConsumerId)
            .OnDelete(DeleteBehavior.SetNull);
        
        builder.HasOne(x => x.Instrument)
            .WithMany(x => x.ConsumerDevices)
            .HasForeignKey(x => x.InstrumentId);
        
        builder.Property(x => x.DriverVersion)
            .IsString(15);

        builder.Property(x => x.SerialNumber)
            .IsString(50);
    }
}