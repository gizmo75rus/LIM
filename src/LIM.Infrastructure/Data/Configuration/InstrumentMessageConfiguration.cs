using LIM.ApplicationCore.Models;
using LIM.Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LIM.Infrastructure.Data.Configuration;

public class InstrumentMessageConfiguration : IEntityTypeConfiguration<InstrumentMessage>
{

    public void Configure(EntityTypeBuilder<InstrumentMessage> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x=>x.Id)
            .HasDefaultValueSql("gen_random_uuid()");
        
        builder.Property(x=>x.Received)
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(x => x.Body)
            .IsString(int.MaxValue);
        
        builder.HasOne(x => x.ConsumerInstrument)
            .WithMany(x => x.Messages)
            .HasForeignKey(x => x.ConsumerInstrumentId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}