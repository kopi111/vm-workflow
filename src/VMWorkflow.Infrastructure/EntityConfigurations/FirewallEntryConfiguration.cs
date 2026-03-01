using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VMWorkflow.Domain.Entities;

namespace VMWorkflow.Infrastructure.EntityConfigurations;

public class FirewallEntryConfiguration : IEntityTypeConfiguration<FirewallEntry>
{
    public void Configure(EntityTypeBuilder<FirewallEntry> builder)
    {
        builder.HasKey(f => f.FirewallEntryId);

        builder.Property(f => f.PolicyName).IsRequired().HasMaxLength(200);
        builder.Property(f => f.VDOM).IsRequired().HasMaxLength(200);
        builder.Property(f => f.SourceInterface).HasMaxLength(200);
        builder.Property(f => f.DestinationInterface).HasMaxLength(200);
        builder.Property(f => f.SourceIP).HasMaxLength(100);
        builder.Property(f => f.DestinationIP).HasMaxLength(100);
        builder.Property(f => f.Services).HasMaxLength(500);
        builder.Property(f => f.Schedule).HasMaxLength(100);
        builder.Property(f => f.Action).IsRequired().HasConversion<string>().HasMaxLength(20);
    }
}
