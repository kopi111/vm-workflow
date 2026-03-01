using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VMWorkflow.Domain.Entities;

namespace VMWorkflow.Infrastructure.EntityConfigurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(a => a.AuditLogId);

        builder.Property(a => a.User).IsRequired().HasMaxLength(100);
        builder.Property(a => a.HttpMethod).IsRequired().HasMaxLength(10);
        builder.Property(a => a.Path).IsRequired().HasMaxLength(500);
        builder.Property(a => a.StatusCode).IsRequired();

        builder.HasIndex(a => a.Timestamp);
        builder.HasIndex(a => a.User);
    }
}
