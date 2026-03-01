namespace VMWorkflow.Domain.Entities;

public class FirewallServiceEntry
{
    public Guid FirewallServiceEntryId { get; set; }
    public Guid FirewallEntryId { get; set; }

    public string Port { get; set; } = string.Empty;
    public string Protocol { get; set; } = string.Empty;
    public string? ServiceName { get; set; }

    // Navigation
    public FirewallEntry FirewallEntry { get; set; } = null!;
}
