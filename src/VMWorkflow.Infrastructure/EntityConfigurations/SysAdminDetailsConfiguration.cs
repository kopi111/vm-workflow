using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VMWorkflow.Domain.Entities;

namespace VMWorkflow.Infrastructure.EntityConfigurations;

public class SysAdminDetailsConfiguration : IEntityTypeConfiguration<SysAdminDetails>
{
    public void Configure(EntityTypeBuilder<SysAdminDetails> builder)
    {
        builder.HasKey(s => s.SysAdminDetailsId);

        builder.HasIndex(s => s.RequestId).IsUnique();

        builder.Property(s => s.SensitivityLevel).IsRequired().HasMaxLength(100);
        builder.Property(s => s.ServerResources).HasMaxLength(50);
        builder.Property(s => s.WebServer).IsRequired().HasMaxLength(50);
        builder.Property(s => s.DatabaseNameType).HasMaxLength(20).HasDefaultValue("none");
        builder.Property(s => s.DatabaseName).HasMaxLength(200);
        builder.Property(s => s.DatabaseUsername).HasMaxLength(200);
        builder.Property(s => s.Hostname).IsRequired().HasMaxLength(200);
        builder.Property(s => s.SubmittedBy).IsRequired().HasMaxLength(100);

        builder.HasMany(s => s.Services)
            .WithOne(se => se.SysAdminDetails)
            .HasForeignKey(se => se.SysAdminDetailsId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
