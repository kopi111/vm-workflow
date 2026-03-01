using VMWorkflow.Domain.Enums;

namespace VMWorkflow.Domain.Entities;

public class Request
{
    public Guid RequestId { get; set; }
    public string ApplicationName { get; set; } = string.Empty;
    public EnvironmentType Environment { get; set; }
    public string ObjectSlug { get; set; } = string.Empty;

    // Developer onboarding fields
    public string? ProgrammingLanguage { get; set; }
    public string? Framework { get; set; }
    public string? Purpose { get; set; }
    public int? ExpectedUsers { get; set; }
    public string? DBMS { get; set; }
    public string? GitRepoLink { get; set; }
    public string? AccessGroup { get; set; }
    public SLALevel SLA { get; set; } = SLALevel.Standard;
    public string? FQDNSuggestion { get; set; }
    public string? AuthenticationMethod { get; set; }

    public RequestStatus Status { get; set; } = RequestStatus.Draft;
    public ExternalSyncStatus ExternalSyncStatus { get; set; } = ExternalSyncStatus.NotStarted;
    public string? NetBoxId { get; set; }
    public string? FortiGatePolicyId { get; set; }

    // IOC Manager
    public string? IocComments { get; set; }

    // Approval tracking (any 2 of 3: CISO, CTO, Ops Officer)
    public string? CisoDecision { get; set; }
    public string? CisoComments { get; set; }
    public string? CisoApprovedBy { get; set; }
    public DateTime? CisoApprovedAt { get; set; }

    public string? CtoDecision { get; set; }
    public string? CtoComments { get; set; }
    public string? CtoApprovedBy { get; set; }
    public DateTime? CtoApprovedAt { get; set; }

    public string? OpsDecision { get; set; }
    public string? OpsComments { get; set; }
    public string? OpsApprovedBy { get; set; }
    public DateTime? OpsApprovedAt { get; set; }

    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public SysAdminDetails? SysAdminDetails { get; set; }
    public DataCenterDetails? DataCenterDetails { get; set; }
    public NOCDetails? NOCDetails { get; set; }
    public SOCDetails? SOCDetails { get; set; }
    public ICollection<StatusHistory> StatusHistories { get; set; } = new List<StatusHistory>();
    public ICollection<AutomationLog> AutomationLogs { get; set; } = new List<AutomationLog>();
}
