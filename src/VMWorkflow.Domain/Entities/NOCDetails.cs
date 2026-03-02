namespace VMWorkflow.Domain.Entities;

public class NOCDetails
{
    public Guid NOCDetailsId { get; set; }
    public Guid RequestId { get; set; }

    public string IPAddress { get; set; } = string.Empty;
    public string SubnetMask { get; set; } = string.Empty;
    public string VLANID { get; set; } = string.Empty;
    public string Gateway { get; set; } = string.Empty;
    public string Port { get; set; } = string.Empty;
    public string VIP { get; set; } = string.Empty;
    public string FQDN { get; set; } = string.Empty;
    public string VirtualIP { get; set; } = string.Empty;
    public string VirtualPort { get; set; } = string.Empty;
    public string VirtualFQDN { get; set; } = string.Empty;

    public string SubmittedBy { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Request Request { get; set; } = null!;
    public ICollection<NetworkPathEntry> NetworkPaths { get; set; } = new List<NetworkPathEntry>();
}
