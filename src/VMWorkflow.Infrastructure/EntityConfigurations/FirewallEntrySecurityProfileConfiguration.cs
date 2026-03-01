using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VMWorkflow.Domain.Entities;

namespace VMWorkflow.Infrastructure.EntityConfigurations;

public class FirewallEntrySecurityProfileConfiguration : IEntityTypeConfiguration<FirewallEntrySecurityProfile>
{
    public void Configure(EntityTypeBuilder<FirewallEntrySecurityProfile> builder)
    {
        builder.HasKey(x => new { x.FirewallEntryId, x.SecurityProfileId });

        builder.HasOne(x => x.FirewallEntry)
            .WithMany(f => f.SecurityProfiles)
            .HasForeignKey(x => x.FirewallEntryId);

        builder.HasOne(x => x.SecurityProfile)
            .WithMany()
            .HasForeignKey(x => x.SecurityProfileId);
    }
}
