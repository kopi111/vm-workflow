using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VMWorkflow.Domain.Entities;

namespace VMWorkflow.Infrastructure.EntityConfigurations;

public class NetworkPathEntryConfiguration : IEntityTypeConfiguration<NetworkPathEntry>
{
    public void Configure(EntityTypeBuilder<NetworkPathEntry> builder)
    {
        builder.HasKey(n => n.NetworkPathEntryId);

        builder.Property(n => n.SwitchName).IsRequired().HasMaxLength(100);
        builder.Property(n => n.Port).IsRequired().HasMaxLength(50);
        builder.Property(n => n.LinkSpeed).HasMaxLength(20);
    }
}
