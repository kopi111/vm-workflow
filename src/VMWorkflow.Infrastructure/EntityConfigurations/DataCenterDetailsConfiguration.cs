using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VMWorkflow.Domain.Entities;

namespace VMWorkflow.Infrastructure.EntityConfigurations;

public class DataCenterDetailsConfiguration : IEntityTypeConfiguration<DataCenterDetails>
{
    public void Configure(EntityTypeBuilder<DataCenterDetails> builder)
    {
        builder.HasKey(d => d.DataCenterDetailsId);

        builder.HasIndex(d => d.RequestId).IsUnique();

        builder.Property(d => d.Environment).IsRequired().HasMaxLength(30);
        builder.Property(d => d.UplinkSpeed).IsRequired().HasMaxLength(20);
        builder.Property(d => d.BareMetalType).IsRequired().HasMaxLength(30);
        builder.Property(d => d.PortNumber).IsRequired().HasMaxLength(15);
        builder.Property(d => d.DC).IsRequired().HasMaxLength(50);
        builder.Property(d => d.RackRoom).IsRequired().HasMaxLength(50);
        builder.Property(d => d.RackNumber).IsRequired().HasMaxLength(20);
        builder.Property(d => d.Cluster).IsRequired().HasMaxLength(50);
        builder.Property(d => d.SubmittedBy).IsRequired().HasMaxLength(64);
    }
}
