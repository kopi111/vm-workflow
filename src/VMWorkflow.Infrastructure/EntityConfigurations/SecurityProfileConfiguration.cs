using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VMWorkflow.Domain.Entities;

namespace VMWorkflow.Infrastructure.EntityConfigurations;

public class SecurityProfileConfiguration : IEntityTypeConfiguration<SecurityProfile>
{
    public void Configure(EntityTypeBuilder<SecurityProfile> builder)
    {
        builder.HasKey(s => s.SecurityProfileId);
        builder.Property(s => s.Name).IsRequired().HasMaxLength(100);
        builder.Property(s => s.CreatedBy).HasMaxLength(200);
        builder.HasIndex(s => s.Name).IsUnique();
    }
}
