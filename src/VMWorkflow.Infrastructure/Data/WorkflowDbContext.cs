using Microsoft.EntityFrameworkCore;
using VMWorkflow.Domain.Entities;
using VMWorkflow.Infrastructure.EntityConfigurations;

namespace VMWorkflow.Infrastructure.Data;

public class WorkflowDbContext : DbContext
{
    public WorkflowDbContext(DbContextOptions<WorkflowDbContext> options) : base(options) { }

    public DbSet<Request> Requests => Set<Request>();
    public DbSet<SysAdminDetails> SysAdminDetails => Set<SysAdminDetails>();
    public DbSet<DataCenterDetails> DataCenterDetails => Set<DataCenterDetails>();
    public DbSet<NOCDetails> NOCDetails => Set<NOCDetails>();
    public DbSet<SOCDetails> SOCDetails => Set<SOCDetails>();
    public DbSet<NetworkPathEntry> NetworkPathEntries => Set<NetworkPathEntry>();
    public DbSet<FirewallEntry> FirewallEntries => Set<FirewallEntry>();
    public DbSet<FirewallServiceEntry> FirewallServiceEntries => Set<FirewallServiceEntry>();
    public DbSet<StatusHistory> StatusHistories => Set<StatusHistory>();
    public DbSet<AutomationLog> AutomationLogs => Set<AutomationLog>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<User> Users => Set<User>();
    public DbSet<ResourceGroup> ResourceGroups => Set<ResourceGroup>();
    public DbSet<SecurityProfile> SecurityProfiles => Set<SecurityProfile>();
    public DbSet<Vdom> Vdoms => Set<Vdom>();
    public DbSet<FirewallEntrySecurityProfile> FirewallEntrySecurityProfiles => Set<FirewallEntrySecurityProfile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new RequestConfiguration());
        modelBuilder.ApplyConfiguration(new SysAdminDetailsConfiguration());
        modelBuilder.ApplyConfiguration(new DataCenterDetailsConfiguration());
        modelBuilder.ApplyConfiguration(new NOCDetailsConfiguration());
        modelBuilder.ApplyConfiguration(new SOCDetailsConfiguration());
        modelBuilder.ApplyConfiguration(new NetworkPathEntryConfiguration());
        modelBuilder.ApplyConfiguration(new FirewallEntryConfiguration());
        modelBuilder.ApplyConfiguration(new FirewallServiceEntryConfiguration());
        modelBuilder.ApplyConfiguration(new StatusHistoryConfiguration());
        modelBuilder.ApplyConfiguration(new AutomationLogConfiguration());
        modelBuilder.ApplyConfiguration(new AuditLogConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new ResourceGroupConfiguration());
        modelBuilder.ApplyConfiguration(new SecurityProfileConfiguration());
        modelBuilder.ApplyConfiguration(new FirewallEntrySecurityProfileConfiguration());
        modelBuilder.ApplyConfiguration(new VdomConfiguration());

        base.OnModelCreating(modelBuilder);
    }
}
