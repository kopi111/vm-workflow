using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VMWorkflow.Domain.Entities;

namespace VMWorkflow.Infrastructure.EntityConfigurations;

public class AutomationLogConfiguration : IEntityTypeConfiguration<AutomationLog>
{
    public void Configure(EntityTypeBuilder<AutomationLog> builder)
    {
        builder.HasKey(a => a.AutomationLogId);


        builder.Property(a => a.Action).IsRequired().HasMaxLength(200);
        builder.Property(a => a.Payload);
        builder.Property(a => a.Response);
    }
}
