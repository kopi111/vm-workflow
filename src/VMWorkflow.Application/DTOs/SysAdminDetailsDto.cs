namespace VMWorkflow.Application.DTOs;

public class SysAdminDetailsDto
{
    public string SensitivityLevel { get; set; } = string.Empty;
    public string ServerResources { get; set; } = string.Empty;
    public string WebServer { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string DatabaseUsername { get; set; } = string.Empty;
    public string Hostname { get; set; } = string.Empty;
    public List<ServiceEntryDto> Services { get; set; } = new();
}
