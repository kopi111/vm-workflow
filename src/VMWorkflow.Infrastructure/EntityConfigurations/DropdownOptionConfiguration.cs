using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VMWorkflow.Domain.Entities;

namespace VMWorkflow.Infrastructure.EntityConfigurations;

public class DropdownOptionConfiguration : IEntityTypeConfiguration<DropdownOption>
{
    public void Configure(EntityTypeBuilder<DropdownOption> builder)
    {
        builder.HasKey(d => d.DropdownOptionId);
        builder.Property(d => d.Category).IsRequired().HasMaxLength(50);
        builder.Property(d => d.Value).IsRequired().HasMaxLength(100);
        builder.Property(d => d.CreatedBy).HasMaxLength(200);
        builder.HasIndex(d => new { d.Category, d.Value }).IsUnique();
    }
}
