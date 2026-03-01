using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VMWorkflow.Domain.Entities;

namespace VMWorkflow.Infrastructure.EntityConfigurations;

public class RequestConfiguration : IEntityTypeConfiguration<Request>
{
    public void Configure(EntityTypeBuilder<Request> builder)
    {
        builder.HasKey(r => r.RequestId);

        builder.Property(r => r.ApplicationName).IsRequired().HasMaxLength(200);
        builder.Property(r => r.ObjectSlug).IsRequired().HasMaxLength(150);
        builder.HasIndex(r => r.ObjectSlug).IsUnique();

        builder.Property(r => r.Environment).HasConversion<string>().HasMaxLength(50);
        builder.Property(r => r.Status).HasConversion<string>().HasMaxLength(50);
        builder.Property(r => r.ExternalSyncStatus).HasConversion<string>().HasMaxLength(50);
        builder.Property(r => r.SLA).HasConversion<string>().HasMaxLength(50);

        builder.Property(r => r.ProgrammingLanguage).HasMaxLength(100);
        builder.Property(r => r.Framework).HasMaxLength(200);
        builder.Property(r => r.Purpose).HasMaxLength(2000);
        builder.Property(r => r.DBMS).HasMaxLength(100);
        builder.Property(r => r.GitRepoLink).HasMaxLength(500);
        builder.Property(r => r.AccessGroup).HasMaxLength(200);
        builder.Property(r => r.FQDNSuggestion).HasMaxLength(300);
        builder.Property(r => r.AuthenticationMethod).HasMaxLength(200);

        builder.Property(r => r.NetBoxId).HasMaxLength(100);
        builder.Property(r => r.FortiGatePolicyId).HasMaxLength(100);
        builder.Property(r => r.CreatedBy).IsRequired().HasMaxLength(100);

        builder.HasOne(r => r.SysAdminDetails).WithOne(s => s.Request).HasForeignKey<SysAdminDetails>(s => s.RequestId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(r => r.DataCenterDetails).WithOne(d => d.Request).HasForeignKey<DataCenterDetails>(d => d.RequestId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(r => r.NOCDetails).WithOne(n => n.Request).HasForeignKey<NOCDetails>(n => n.RequestId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(r => r.SOCDetails).WithOne(s => s.Request).HasForeignKey<SOCDetails>(s => s.RequestId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(r => r.StatusHistories).WithOne(s => s.Request).HasForeignKey(s => s.RequestId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(r => r.AutomationLogs).WithOne(a => a.Request).HasForeignKey(a => a.RequestId).OnDelete(DeleteBehavior.Cascade);
    }
}
