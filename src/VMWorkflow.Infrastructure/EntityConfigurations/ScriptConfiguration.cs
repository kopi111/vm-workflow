using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VMWorkflow.Domain.Entities;

namespace VMWorkflow.Infrastructure.EntityConfigurations;

public class ScriptConfiguration : IEntityTypeConfiguration<Script>
{
    public void Configure(EntityTypeBuilder<Script> builder)
    {
        builder.HasKey(s => s.ScriptId);

        builder.Property(s => s.ScriptType).IsRequired().HasMaxLength(50);
        builder.Property(s => s.Content).IsRequired().HasColumnType("text");
        builder.Property(s => s.FileName).IsRequired().HasMaxLength(250);
        builder.Property(s => s.GeneratedBy).IsRequired().HasMaxLength(100);

        builder.HasIndex(s => s.RequestId);
        builder.HasIndex(s => s.GeneratedAt);

        builder.HasOne(s => s.Request)
            .WithMany()
            .HasForeignKey(s => s.RequestId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
