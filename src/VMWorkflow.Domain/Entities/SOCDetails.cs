namespace VMWorkflow.Domain.Entities;

public class SOCDetails
{
    public Guid SOCDetailsId { get; set; }
    public Guid RequestId { get; set; }

    public string SubmittedBy { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Request Request { get; set; } = null!;
    public ICollection<FirewallEntry> FirewallEntries { get; set; } = new List<FirewallEntry>();
}
