using LIM.ApplicationCore.BaseObjects;
using LIM.SharedKernel.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LIM.Infrastructure.Helpers;

public static class EntityTypeBuilderHelper
{
    public static EntityTypeBuilder<TEntity> IsJournaled<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : class, IJournaledEntity
    {
        builder.Property(x=>x.CreatedAt)
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
        
        builder.Property(x => x.CreatedBy)
            .HasMaxLength(50);

        builder.Property(x => x.UpdatedAt)
            .HasColumnType("timestamp");

        builder.Property(x => x.UpdatedBy)
            .HasMaxLength(50);
            
            
        return builder;
    }

}