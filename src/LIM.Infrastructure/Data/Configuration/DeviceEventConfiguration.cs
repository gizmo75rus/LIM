using LIM.ApplicationCore.Models;
using LIM.Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LIM.Infrastructure.Data.Configuration;

public class DeviceEventConfiguration : IEntityTypeConfiguration<DeviceEvent>
{
    public void Configure(EntityTypeBuilder<DeviceEvent> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder
            .Property(x=>x.Id)
            .HasDefaultValueSql("gen_random_uuid()");
        
        builder.Property(x=>x.CreatedAt)
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
        
        builder.Property(x => x.Message)
            .IsString(int.MaxValue);
        
        builder.HasOne(x => x.ConsumerDevice)
            .WithMany(x => x.DeviceEvents)
            .HasForeignKey(x => x.ConsumerDeviceId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}