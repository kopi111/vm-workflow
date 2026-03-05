using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VMWorkflow.Domain.Entities;

namespace VMWorkflow.Infrastructure.EntityConfigurations;

public class StatusHistoryConfiguration : IEntityTypeConfiguration<StatusHistory>
{
    public void Configure(EntityTypeBuilder<StatusHistory> builder)
    {
        builder.HasKey(s => s.StatusHistoryId);


        builder.Property(s => s.OldStatus).HasConversion<string>().HasMaxLength(50);
        builder.Property(s => s.NewStatus).HasConversion<string>().HasMaxLength(50);
        builder.Property(s => s.ChangedBy).IsRequired().HasMaxLength(100);
        builder.Property(s => s.Comments).HasMaxLength(1000);

        builder.HasIndex(s => new { s.RequestId, s.Timestamp });
    }
}
