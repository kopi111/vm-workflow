namespace VMWorkflow.Application.DTOs;

public class FirewallServiceEntryDto
{
    public string Port { get; set; } = string.Empty;
    public string Protocol { get; set; } = string.Empty;
    public string? ServiceName { get; set; }
}
