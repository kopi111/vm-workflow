using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VMWorkflow.Domain.Entities;

namespace VMWorkflow.Infrastructure.EntityConfigurations;

public class SOCDetailsConfiguration : IEntityTypeConfiguration<SOCDetails>
{
    public void Configure(EntityTypeBuilder<SOCDetails> builder)
    {
        builder.HasKey(s => s.SOCDetailsId);

        builder.HasIndex(s => s.RequestId).IsUnique();

        builder.Property(s => s.SubmittedBy).IsRequired().HasMaxLength(100);

        builder.HasMany(s => s.FirewallEntries).WithOne(f => f.SOCDetails).HasForeignKey(f => f.SOCDetailsId).OnDelete(DeleteBehavior.Cascade);
    }
}
