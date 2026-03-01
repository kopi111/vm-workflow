using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VMWorkflow.Domain.Entities;

namespace VMWorkflow.Infrastructure.EntityConfigurations;

public class ResourceGroupConfiguration : IEntityTypeConfiguration<ResourceGroup>
{
    public void Configure(EntityTypeBuilder<ResourceGroup> builder)
    {
        builder.HasKey(r => r.ResourceGroupId);
        builder.Property(r => r.Name).IsRequired().HasMaxLength(50);
        builder.Property(r => r.Ram).IsRequired().HasMaxLength(20);
        builder.Property(r => r.Hdd).IsRequired().HasMaxLength(20);
        builder.Property(r => r.CreatedBy).HasMaxLength(200);
        builder.HasIndex(r => r.Name).IsUnique();
    }
}
