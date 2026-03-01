using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VMWorkflow.Domain.Entities;

namespace VMWorkflow.Infrastructure.EntityConfigurations;

public class FirewallServiceEntryConfiguration : IEntityTypeConfiguration<FirewallServiceEntry>
{
    public void Configure(EntityTypeBuilder<FirewallServiceEntry> builder)
    {
        builder.HasKey(s => s.FirewallServiceEntryId);

        builder.Property(s => s.Port).IsRequired().HasMaxLength(10);
        builder.Property(s => s.Protocol).IsRequired().HasMaxLength(10);
        builder.Property(s => s.ServiceName).HasMaxLength(100);
    }
}
