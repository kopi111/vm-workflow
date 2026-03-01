namespace VMWorkflow.Web.Models;

public class AuditLogEntry
{
    public Guid AuditLogId { get; set; }
    public string User { get; set; } = string.Empty;
    public string HttpMethod { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public DateTime Timestamp { get; set; }
}

public class StatusHistoryLogEntry
{
    public Guid StatusHistoryId { get; set; }
    public Guid RequestId { get; set; }
    public string RequestNumber { get; set; } = string.Empty;
    public string OldStatus { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;
    public string ChangedBy { get; set; } = string.Empty;
    public string? Comments { get; set; }
    public DateTime Timestamp { get; set; }
}

public class UserModel
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsBlocked { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateUserModel
{
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? Password { get; set; }
}

public class LoginModel
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RegisterModel
{
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = "Requester";
    public string Password { get; set; } = string.Empty;
}

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

public class CreateRequestModel
{
    public string ApplicationName { get; set; } = string.Empty;
    public string Environment { get; set; } = "Development";
    public string? ProgrammingLanguage { get; set; }
    public string? Framework { get; set; }
    public string? Purpose { get; set; }
    public int? ExpectedUsers { get; set; }
    public string? DBMS { get; set; }
    public string? GitRepoLink { get; set; }
    public string? AccessGroup { get; set; }
    public string SLA { get; set; } = "Standard";
    public string? FQDNSuggestion { get; set; }
    public string? AuthenticationMethod { get; set; }
}

public class UpdateRequestModel
{
    public string? ApplicationName { get; set; }
    public string? ProgrammingLanguage { get; set; }
    public string? Framework { get; set; }
    public string? Purpose { get; set; }
    public int? ExpectedUsers { get; set; }
    public string? DBMS { get; set; }
    public string? GitRepoLink { get; set; }
    public string? AccessGroup { get; set; }
    public string? SLA { get; set; }
    public string? FQDNSuggestion { get; set; }
    public string? AuthenticationMethod { get; set; }
}

public class SysAdminDetailsModel
{
    public string SensitivityLevel { get; set; } = string.Empty;
    public string ServerResources { get; set; } = string.Empty;
    public string WebServer { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string DatabaseUsername { get; set; } = string.Empty;
    public string Hostname { get; set; } = string.Empty;
}

public class DataCenterDetailsModel
{
    public string Environment { get; set; } = string.Empty;
    public string UplinkSpeed { get; set; } = string.Empty;
    public string BareMetalType { get; set; } = string.Empty;
    public string PortNumber { get; set; } = string.Empty;
    public string DC { get; set; } = string.Empty;
    public string RackRoom { get; set; } = string.Empty;
    public string RackNumber { get; set; } = string.Empty;
    public string Cluster { get; set; } = string.Empty;
}

public class NOCDetailsModel
{
    public string IPAddress { get; set; } = string.Empty;
    public string SubnetMask { get; set; } = string.Empty;
    public string VLANID { get; set; } = string.Empty;
    public string Gateway { get; set; } = string.Empty;
    public string Port { get; set; } = string.Empty;
    public string VIP { get; set; } = string.Empty;
    public string FQDN { get; set; } = string.Empty;
    public List<NetworkPathEntryModel> NetworkPaths { get; set; } = new();
}

public class NetworkPathEntryModel
{
    public string SwitchName { get; set; } = string.Empty;
    public string Port { get; set; } = string.Empty;
    public string LinkSpeed { get; set; } = string.Empty;
}

public class SOCDetailsModel
{
    public List<FirewallEntryModel> FirewallEntries { get; set; } = new();
}

public class FirewallEntryModel
{
    public string PolicyName { get; set; } = string.Empty;
    public string VDOM { get; set; } = string.Empty;
    public string SourceInterface { get; set; } = string.Empty;
    public string DestinationInterface { get; set; } = string.Empty;
    public string SourceIP { get; set; } = string.Empty;
    public string DestinationIP { get; set; } = string.Empty;
    public string Services { get; set; } = string.Empty;
    public string Schedule { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public List<Guid> SecurityProfileIds { get; set; } = new();
    public List<string> SecurityProfileNames { get; set; } = new();
}

public class ApprovalModel
{
    public string Decision { get; set; } = string.Empty; // Approve, Reject, Return
    public string? Comments { get; set; }
}

public class SendBackModel
{
    public string Comments { get; set; } = string.Empty;
}

public class StatusHistoryEntry
{
    public string OldStatus { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;
    public string ChangedBy { get; set; } = string.Empty;
    public string? Comments { get; set; }
    public DateTime Timestamp { get; set; }
}

public class RequestResponse
{
    public Guid RequestId { get; set; }
    public string ApplicationName { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public string ObjectSlug { get; set; } = string.Empty;
    public string? ProgrammingLanguage { get; set; }
    public string? Framework { get; set; }
    public string? Purpose { get; set; }
    public int? ExpectedUsers { get; set; }
    public string? DBMS { get; set; }
    public string? GitRepoLink { get; set; }
    public string? AccessGroup { get; set; }
    public string SLA { get; set; } = "Standard";
    public string? FQDNSuggestion { get; set; }
    public string? AuthenticationMethod { get; set; }
    public string Status { get; set; } = string.Empty;

    // IOC Manager
    public string? IocComments { get; set; }

    // Approval tracking
    public string? CisoDecision { get; set; }
    public string? CisoComments { get; set; }
    public string? CisoApprovedBy { get; set; }
    public string? CtoDecision { get; set; }
    public string? CtoComments { get; set; }
    public string? CtoApprovedBy { get; set; }
    public string? OpsDecision { get; set; }
    public string? OpsComments { get; set; }
    public string? OpsApprovedBy { get; set; }

    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public SysAdminDetailsModel? SysAdminDetails { get; set; }
    public DataCenterDetailsModel? DataCenterDetails { get; set; }
    public NOCDetailsModel? NOCDetails { get; set; }
    public SOCDetailsModel? SOCDetails { get; set; }
    public List<StatusHistoryEntry> StatusHistories { get; set; } = new();
}
