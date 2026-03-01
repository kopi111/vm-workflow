namespace VMWorkflow.Domain.Entities;

public class FirewallEntrySecurityProfile
{
    public Guid FirewallEntryId { get; set; }
    public Guid SecurityProfileId { get; set; }

    public FirewallEntry FirewallEntry { get; set; } = null!;
    public SecurityProfile SecurityProfile { get; set; } = null!;
}
