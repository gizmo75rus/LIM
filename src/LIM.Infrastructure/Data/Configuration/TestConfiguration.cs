using LIM.ApplicationCore.Entities;
using LIM.Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LIM.Infrastructure.Data.Configuration;

public class TestConfiguration : IEntityTypeConfiguration<Test>
{
    public void Configure(EntityTypeBuilder<Test> builder)
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
            .WithMany(x => x.Tests)
            .HasForeignKey(x => x.ConsumerDeviceId)
            .OnDelete(DeleteBehavior.SetNull);


    }
}