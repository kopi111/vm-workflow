using VMWorkflow.Domain.Enums;

namespace VMWorkflow.Domain.Entities;

public class FirewallEntry
{
    public Guid FirewallEntryId { get; set; }
    public Guid SOCDetailsId { get; set; }

    public string PolicyName { get; set; } = string.Empty;
    public string VDOM { get; set; } = string.Empty;
    public string SourceInterface { get; set; } = string.Empty;
    public string DestinationInterface { get; set; } = string.Empty;
    public string SourceIP { get; set; } = string.Empty;
    public string DestinationIP { get; set; } = string.Empty;
    public string Services { get; set; } = string.Empty;
    public string Schedule { get; set; } = string.Empty;
    public PolicyAction Action { get; set; }

    // Navigation
    public SOCDetails SOCDetails { get; set; } = null!;
    public ICollection<FirewallEntrySecurityProfile> SecurityProfiles { get; set; } = new List<FirewallEntrySecurityProfile>();
}
