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
    public DbSet<DropdownOption> DropdownOptions => Set<DropdownOption>();
    public DbSet<Script> Scripts => Set<Script>();

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
        modelBuilder.ApplyConfiguration(new DropdownOptionConfiguration());
        modelBuilder.ApplyConfiguration(new ScriptConfiguration());

        base.OnModelCreating(modelBuilder);
    }

    public async Task SeedDefaultUsersAsync()
    {
        if (await Users.AnyAsync())
            return;

        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Password@123");
        var now = DateTime.UtcNow;

        var defaultUsers = new List<User>
        {
            new() { UserId = Guid.NewGuid(), Username = "admin", DisplayName = "Platform Admin", Email = "admin@vmworkflow.local", Role = "PlatformAdmin", PasswordHash = passwordHash, CreatedAt = now, UpdatedAt = now },
            new() { UserId = Guid.NewGuid(), Username = "sysadmin", DisplayName = "System Admin", Email = "sysadmin@vmworkflow.local", Role = "SysAdmin", PasswordHash = passwordHash, CreatedAt = now, UpdatedAt = now },
            new() { UserId = Guid.NewGuid(), Username = "requester", DisplayName = "Requester User", Email = "requester@vmworkflow.local", Role = "Requester", PasswordHash = passwordHash, CreatedAt = now, UpdatedAt = now },
            new() { UserId = Guid.NewGuid(), Username = "datacenter", DisplayName = "DataCenter User", Email = "datacenter@vmworkflow.local", Role = "DataCenter", PasswordHash = passwordHash, CreatedAt = now, UpdatedAt = now },
            new() { UserId = Guid.NewGuid(), Username = "noc", DisplayName = "NOC User", Email = "noc@vmworkflow.local", Role = "NOC", PasswordHash = passwordHash, CreatedAt = now, UpdatedAt = now },
            new() { UserId = Guid.NewGuid(), Username = "soc", DisplayName = "SOC User", Email = "soc@vmworkflow.local", Role = "SOC", PasswordHash = passwordHash, CreatedAt = now, UpdatedAt = now },
            new() { UserId = Guid.NewGuid(), Username = "iocmanager", DisplayName = "IOC Manager", Email = "iocmanager@vmworkflow.local", Role = "IOCManager", PasswordHash = passwordHash, CreatedAt = now, UpdatedAt = now },
            new() { UserId = Guid.NewGuid(), Username = "ciso", DisplayName = "CISO Officer", Email = "ciso@vmworkflow.local", Role = "CISO", PasswordHash = passwordHash, CreatedAt = now, UpdatedAt = now },
            new() { UserId = Guid.NewGuid(), Username = "ops", DisplayName = "Ops Manager", Email = "ops@vmworkflow.local", Role = "Ops", PasswordHash = passwordHash, CreatedAt = now, UpdatedAt = now },
            new() { UserId = Guid.NewGuid(), Username = "developer", DisplayName = "Developer", Email = "developer@vmworkflow.local", Role = "Requester", PasswordHash = passwordHash, CreatedAt = now, UpdatedAt = now },
        };

        Users.AddRange(defaultUsers);
        await SaveChangesAsync();
    }

    public async Task SeedDefaultDropdownOptionsAsync()
    {
        if (await DropdownOptions.AnyAsync())
            return;

        var seed = new Dictionary<string, string[]>
        {
            ["SensitivityLevel"] = new[] { "Low", "Medium", "High", "Critical" },
            ["WebServer"] = new[] { "IIS", "Apache", "Nginx" },
            ["DBMS"] = new[] { "PostgreSQL", "MySQL", "SQL Server", "Oracle", "MongoDB", "MariaDB", "SQLite", "None" },
            ["SLA"] = new[] { "Standard", "Critical", "MissionCritical" },
            ["Environment"] = new[] { "Development", "Staging", "Production", "DR" },
            ["ServerEnvironment"] = new[] { "Dell", "HyperV" },
            ["BareMetalType"] = new[] { "VM", "Physical" },
            ["Cluster"] = new[] { "HyperFlex", "VxRail" },
        };

        var options = new List<DropdownOption>();
        foreach (var (category, values) in seed)
        {
            for (int i = 0; i < values.Length; i++)
            {
                options.Add(new DropdownOption
                {
                    DropdownOptionId = Guid.NewGuid(),
                    Category = category,
                    Value = values[i],
                    SortOrder = i,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        DropdownOptions.AddRange(options);
        await SaveChangesAsync();
    }

    public async Task SeedDefaultAdminDataAsync()
    {
        if (!await Vdoms.AnyAsync())
        {
            Vdoms.AddRange(
                new Vdom { VdomId = Guid.NewGuid(), Name = "root", CreatedBy = "system", CreatedAt = DateTime.UtcNow },
                new Vdom { VdomId = Guid.NewGuid(), Name = "DMZ", CreatedBy = "system", CreatedAt = DateTime.UtcNow },
                new Vdom { VdomId = Guid.NewGuid(), Name = "Internal", CreatedBy = "system", CreatedAt = DateTime.UtcNow },
                new Vdom { VdomId = Guid.NewGuid(), Name = "Guest", CreatedBy = "system", CreatedAt = DateTime.UtcNow }
            );
            await SaveChangesAsync();
        }

        if (!await SecurityProfiles.AnyAsync())
        {
            SecurityProfiles.AddRange(
                new SecurityProfile { SecurityProfileId = Guid.NewGuid(), Name = "Web-Filter-Default", CreatedBy = "system", CreatedAt = DateTime.UtcNow },
                new SecurityProfile { SecurityProfileId = Guid.NewGuid(), Name = "AV-Default", CreatedBy = "system", CreatedAt = DateTime.UtcNow },
                new SecurityProfile { SecurityProfileId = Guid.NewGuid(), Name = "IPS-Default", CreatedBy = "system", CreatedAt = DateTime.UtcNow },
                new SecurityProfile { SecurityProfileId = Guid.NewGuid(), Name = "App-Control-Default", CreatedBy = "system", CreatedAt = DateTime.UtcNow }
            );
            await SaveChangesAsync();
        }

        if (!await ResourceGroups.AnyAsync())
        {
            ResourceGroups.AddRange(
                new ResourceGroup { ResourceGroupId = Guid.NewGuid(), Name = "Production-RG", CreatedBy = "system", CreatedAt = DateTime.UtcNow },
                new ResourceGroup { ResourceGroupId = Guid.NewGuid(), Name = "Staging-RG", CreatedBy = "system", CreatedAt = DateTime.UtcNow },
                new ResourceGroup { ResourceGroupId = Guid.NewGuid(), Name = "Development-RG", CreatedBy = "system", CreatedAt = DateTime.UtcNow }
            );
            await SaveChangesAsync();
        }
    }
}
