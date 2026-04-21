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

        builder.Property(n => n.IPAddress).IsRequired().HasMaxLength(45);
        builder.Property(n => n.SubnetMask).IsRequired().HasMaxLength(15);
        builder.Property(n => n.VLANID).IsRequired().HasMaxLength(10);
        builder.Property(n => n.Gateway).IsRequired().HasMaxLength(45);
        builder.Property(n => n.Port).IsRequired().HasMaxLength(15);
        builder.Property(n => n.VIP).HasMaxLength(45);
        builder.Property(n => n.VirtualIP).HasMaxLength(45);
        builder.Property(n => n.VirtualPort).HasMaxLength(15);
        builder.Property(n => n.VirtualFQDN).HasMaxLength(253);
        builder.Property(n => n.SubmittedBy).IsRequired().HasMaxLength(64);

        builder.HasMany(n => n.NetworkPaths).WithOne(np => np.NOCDetails).HasForeignKey(np => np.NOCDetailsId).OnDelete(DeleteBehavior.Cascade);
    }
}
