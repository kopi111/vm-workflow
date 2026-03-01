namespace VMWorkflow.Application.DTOs;

public class FirewallEntryDto
{
    public string PolicyName { get; set; } = string.Empty;
    public string VDOM { get; set; } = string.Empty;
    public string SourceInterface { get; set; } = string.Empty;
    public string DestinationInterface { get; set; } = string.Empty;
    public string SourceIP { get; set; } = string.Empty;
    public string DestinationIP { get; set; } = string.Empty;
    public List<FirewallServiceEntryDto> Services { get; set; } = new();
    public string Schedule { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public List<Guid> SecurityProfileIds { get; set; } = new();
    public List<string> SecurityProfileNames { get; set; } = new();
}
