using LIM.ApplicationCore.Models;
using LIM.Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LIM.Infrastructure.Data.Configuration;

public class ResearchConfiguration : IEntityTypeConfiguration<Research>
{
    public void Configure(EntityTypeBuilder<Research> builder)
    {
        builder.HasKey(x => x.Id);

        builder.IsJournaled();
        
        builder.Property(x => x.Code)
            .IsString(10);
        
        builder.Property(x => x.LisTestId)
            .IsString(50);

        builder.Property(x => x.MisTestId)
            .IsString(50);

        builder.Property(x => x.Name)
            .IsString(250);

        builder.HasOne(x => x.ConsumerDevice)
            .WithMany(x => x.Researchs)
            .HasForeignKey(x => x.ConsumerInstrumentId)
            .OnDelete(DeleteBehavior.SetNull);


    }
}