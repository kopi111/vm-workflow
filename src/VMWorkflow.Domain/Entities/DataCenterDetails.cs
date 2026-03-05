namespace VMWorkflow.Domain.Entities;

public class DataCenterDetails
{
    public Guid DataCenterDetailsId { get; set; }
    public Guid RequestId { get; set; }

    public string Environment { get; set; } = string.Empty;
    public string UplinkSpeed { get; set; } = string.Empty;
    public string BareMetalType { get; set; } = string.Empty;
    public string PortNumber { get; set; } = string.Empty;
    public string DC { get; set; } = string.Empty;
    public string RackRoom { get; set; } = string.Empty;
    public string RackNumber { get; set; } = string.Empty;
    public string Cluster { get; set; } = string.Empty;

    public string SubmittedBy { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Request Request { get; set; } = null!;
}
