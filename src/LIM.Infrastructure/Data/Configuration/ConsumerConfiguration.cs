using LIM.ApplicationCore.Models;
using LIM.Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LIM.Infrastructure.Data.Configuration;

public class ConsumerConfiguration : IEntityTypeConfiguration<Consumer>
{
    public void Configure(EntityTypeBuilder<Consumer> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Name)
            .IsString(100);
    }
}