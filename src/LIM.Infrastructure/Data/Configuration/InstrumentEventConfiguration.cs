using LIM.ApplicationCore.Models;
using LIM.Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LIM.Infrastructure.Data.Configuration;

public class InstrumentEventConfiguration : IEntityTypeConfiguration<InstrumentEvent>
{
    public void Configure(EntityTypeBuilder<InstrumentEvent> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder
            .Property(x=>x.Id)
            .HasDefaultValueSql("gen_random_uuid()");
        
        builder.Property(x=>x.CreatedAt)
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
        
        builder.Property(x => x.Notes)
            .IsString(int.MaxValue);
        
        builder.HasOne(x => x.ConsumerInstrument)
            .WithMany(x => x.Events)
            .HasForeignKey(x => x.ConsumerInstrumentId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}