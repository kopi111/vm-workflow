using System.ComponentModel.DataAnnotations;

namespace VMWorkflow.Application.DTOs;

public class SysAdminDetailsDto
{
    [Required, StringLength(100)]
    public string SensitivityLevel { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string ServerResources { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string WebServer { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string DatabaseName { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string DatabaseUsername { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string Hostname { get; set; } = string.Empty;

    public List<ServiceEntryDto> Services { get; set; } = new();
}
