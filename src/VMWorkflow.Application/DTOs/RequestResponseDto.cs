using VMWorkflow.Domain.Enums;

namespace VMWorkflow.Application.DTOs;

public class RequestResponseDto
{
    public Guid RequestId { get; set; }
    public string ApplicationName { get; set; } = string.Empty;
    public EnvironmentType Environment { get; set; }
    public string ObjectSlug { get; set; } = string.Empty;

    public string? ProgrammingLanguage { get; set; }
    public string? Framework { get; set; }
    public string? Purpose { get; set; }
    public int? ExpectedUsers { get; set; }
    public string? DBMS { get; set; }
    public string? GitRepoLink { get; set; }
    public string? AccessGroup { get; set; }
    public SLALevel SLA { get; set; }
    public string? FQDNSuggestion { get; set; }
    public string? AuthenticationMethod { get; set; }

    public RequestStatus Status { get; set; }
    public ExternalSyncStatus ExternalSyncStatus { get; set; }
    public string? NetBoxId { get; set; }
    public string? FortiGatePolicyId { get; set; }

    // IOC Manager
    public string? IocComments { get; set; }

    // Approval tracking (CISO + Ops Manager)
    public string? CisoDecision { get; set; }
    public string? CisoComments { get; set; }
    public string? CisoApprovedBy { get; set; }
    public string? OpsDecision { get; set; }
    public string? OpsComments { get; set; }
    public string? OpsApprovedBy { get; set; }

    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public SysAdminDetailsDto? SysAdminDetails { get; set; }
    public DataCenterDetailsDto? DataCenterDetails { get; set; }
    public NOCDetailsDto? NOCDetails { get; set; }
    public SOCDetailsDto? SOCDetails { get; set; }
    public List<StatusHistoryDto> StatusHistories { get; set; } = new();
}
