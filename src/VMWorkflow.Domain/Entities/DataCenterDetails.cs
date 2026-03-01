using VMWorkflow.Domain.Enums;

namespace VMWorkflow.Domain.Entities;

public class DataCenterDetails
{
    public Guid DataCenterDetailsId { get; set; }
    public Guid RequestId { get; set; }

    public ServerEnvironment Environment { get; set; }
    public string UplinkSpeed { get; set; } = string.Empty;
    public BareMetalType BareMetalType { get; set; }
    public string PortNumber { get; set; } = string.Empty;
    public string DC { get; set; } = string.Empty;
    public string RackRoom { get; set; } = string.Empty;
    public string RackNumber { get; set; } = string.Empty;
    public ClusterType Cluster { get; set; }

    public string SubmittedBy { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Request Request { get; set; } = null!;
}
