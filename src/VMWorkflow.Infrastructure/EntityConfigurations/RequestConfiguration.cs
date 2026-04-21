using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VMWorkflow.Domain.Entities;

namespace VMWorkflow.Infrastructure.EntityConfigurations;

public class RequestConfiguration : IEntityTypeConfiguration<Request>
{
    public void Configure(EntityTypeBuilder<Request> builder)
    {
        builder.HasKey(r => r.RequestId);

        builder.Property(r => r.ApplicationName).IsRequired().HasMaxLength(100);
        builder.Property(r => r.ObjectSlug).IsRequired().HasMaxLength(100);
        builder.HasIndex(r => r.ObjectSlug).IsUnique();

        builder.Property(r => r.Environment).HasConversion<string>().HasMaxLength(20);
        builder.Property(r => r.Status).HasConversion<string>().HasMaxLength(30);
        builder.Property(r => r.ExternalSyncStatus).HasConversion<string>().HasMaxLength(20);
        builder.Property(r => r.SLA).HasConversion<string>().HasMaxLength(20);

        builder.Property(r => r.ProgrammingLanguage).HasMaxLength(50);
        builder.Property(r => r.Framework).HasMaxLength(100);
        builder.Property(r => r.Purpose).HasColumnType("text");
        builder.Property(r => r.DBMS).HasMaxLength(50);
        builder.Property(r => r.GitRepoLink).HasMaxLength(500);
        builder.Property(r => r.AccessGroup).HasMaxLength(100);
        builder.Property(r => r.FQDNSuggestion).HasMaxLength(253);
        builder.Property(r => r.AuthenticationMethod).HasMaxLength(50);

        // Approval tracking
        builder.Property(r => r.IocComments).HasColumnType("text");
        builder.Property(r => r.CisoDecision).HasMaxLength(10);
        builder.Property(r => r.CisoComments).HasColumnType("text");
        builder.Property(r => r.CisoApprovedBy).HasMaxLength(64);
        builder.Property(r => r.OpsDecision).HasMaxLength(10);
        builder.Property(r => r.OpsComments).HasColumnType("text");
        builder.Property(r => r.OpsApprovedBy).HasMaxLength(64);

        builder.Property(r => r.NetBoxId).HasMaxLength(50);
        builder.Property(r => r.FortiGatePolicyId).HasMaxLength(50);
        builder.Property(r => r.CreatedBy).IsRequired().HasMaxLength(64);

        // Performance indexes
        builder.HasIndex(r => r.Status);
        builder.HasIndex(r => r.CreatedBy);
        builder.HasIndex(r => r.CreatedAt);

        builder.HasOne(r => r.SysAdminDetails).WithOne(s => s.Request).HasForeignKey<SysAdminDetails>(s => s.RequestId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(r => r.DataCenterDetails).WithOne(d => d.Request).HasForeignKey<DataCenterDetails>(d => d.RequestId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(r => r.NOCDetails).WithOne(n => n.Request).HasForeignKey<NOCDetails>(n => n.RequestId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(r => r.SOCDetails).WithOne(s => s.Request).HasForeignKey<SOCDetails>(s => s.RequestId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(r => r.StatusHistories).WithOne(s => s.Request).HasForeignKey(s => s.RequestId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(r => r.AutomationLogs).WithOne(a => a.Request).HasForeignKey(a => a.RequestId).OnDelete(DeleteBehavior.Cascade);
    }
}
