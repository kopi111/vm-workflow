using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VMWorkflow.Domain.Entities;

namespace VMWorkflow.Infrastructure.EntityConfigurations;

public class ScheduleConfiguration : IEntityTypeConfiguration<Schedule>
{
    public void Configure(EntityTypeBuilder<Schedule> builder)
    {
        builder.HasKey(s => s.ScheduleId);
        builder.Property(s => s.Name).IsRequired().HasMaxLength(50);
        builder.Property(s => s.Color).HasMaxLength(9);
        builder.Property(s => s.RecurrenceDays).HasMaxLength(32);
        builder.Property(s => s.CreatedBy).HasMaxLength(64);
        builder.HasIndex(s => s.Name).IsUnique();
    }
}
