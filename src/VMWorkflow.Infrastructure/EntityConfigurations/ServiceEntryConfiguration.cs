using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VMWorkflow.Domain.Entities;

namespace VMWorkflow.Infrastructure.EntityConfigurations;

public class ServiceEntryConfiguration : IEntityTypeConfiguration<ServiceEntry>
{
    public void Configure(EntityTypeBuilder<ServiceEntry> builder)
    {
        builder.HasKey(s => s.ServiceEntryId);

        builder.Property(s => s.ServiceName).IsRequired().HasMaxLength(50);
        builder.Property(s => s.Port).IsRequired().HasMaxLength(5);
        builder.Property(s => s.Protocol).IsRequired().HasMaxLength(10);
    }
}
