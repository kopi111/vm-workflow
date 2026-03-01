namespace VMWorkflow.Domain.Entities;

public class NetworkPathEntry
{
    public Guid NetworkPathEntryId { get; set; }
    public Guid NOCDetailsId { get; set; }

    public string SwitchName { get; set; } = string.Empty;
    public string Port { get; set; } = string.Empty;
    public string LinkSpeed { get; set; } = string.Empty;

    // Navigation
    public NOCDetails NOCDetails { get; set; } = null!;
}
