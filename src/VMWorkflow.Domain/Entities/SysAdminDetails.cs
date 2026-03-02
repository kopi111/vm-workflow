using VMWorkflow.Domain.Enums;

namespace VMWorkflow.Domain.Entities;

public class SysAdminDetails
{
    public Guid SysAdminDetailsId { get; set; }
    public Guid RequestId { get; set; }

    public string SensitivityLevel { get; set; } = string.Empty;
    public ServerResourceSize ServerResources { get; set; }
    public WebServerType WebServer { get; set; }
    public string DatabaseNameType { get; set; } = "none";
    public string DatabaseName { get; set; } = string.Empty;
    public string DatabaseUsername { get; set; } = string.Empty;
    public string Hostname { get; set; } = string.Empty;

    public string SubmittedBy { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Request Request { get; set; } = null!;
    public ICollection<ServiceEntry> Services { get; set; } = new List<ServiceEntry>();
}
