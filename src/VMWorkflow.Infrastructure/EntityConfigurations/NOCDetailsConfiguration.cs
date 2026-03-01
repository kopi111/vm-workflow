using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VMWorkflow.Domain.Entities;

namespace VMWorkflow.Infrastructure.EntityConfigurations;

public class NOCDetailsConfiguration : IEntityTypeConfiguration<NOCDetails>
{
    public void Configure(EntityTypeBuilder<NOCDetails> builder)
    {
        builder.HasKey(n => n.NOCDetailsId);

        builder.HasIndex(n => n.RequestId).IsUnique();

        builder.Property(n => n.IPAddress).IsRequired().HasMaxLength(50);
        builder.Property(n => n.SubnetMask).IsRequired().HasMaxLength(50);
        builder.Property(n => n.VLANID).IsRequired().HasMaxLength(20);
        builder.Property(n => n.Gateway).IsRequired().HasMaxLength(50);
        builder.Property(n => n.Port).IsRequired().HasMaxLength(50);
        builder.Property(n => n.VIP).IsRequired().HasMaxLength(50);
        builder.Property(n => n.FQDN).IsRequired().HasMaxLength(200);
        builder.Property(n => n.SubmittedBy).IsRequired().HasMaxLength(100);

        builder.HasMany(n => n.NetworkPaths).WithOne(np => np.NOCDetails).HasForeignKey(np => np.NOCDetailsId).OnDelete(DeleteBehavior.Cascade);
    }
}
