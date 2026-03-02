using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VMWorkflow.Domain.Entities;

namespace VMWorkflow.Infrastructure.EntityConfigurations;

public class VdomConfiguration : IEntityTypeConfiguration<Vdom>
{
    public void Configure(EntityTypeBuilder<Vdom> builder)
    {
        builder.HasKey(v => v.VdomId);
        builder.Property(v => v.Name).IsRequired().HasMaxLength(100);
        builder.Property(v => v.CreatedBy).HasMaxLength(200);
        builder.HasIndex(v => v.Name).IsUnique();
    }
}
