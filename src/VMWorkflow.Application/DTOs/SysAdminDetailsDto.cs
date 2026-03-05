using System.ComponentModel.DataAnnotations;

namespace VMWorkflow.Application.DTOs;

public class SysAdminDetailsDto
{
    [Required(ErrorMessage = "Sensitivity Level is required."), StringLength(100)]
    public string SensitivityLevel { get; set; } = string.Empty;

    [StringLength(100)]
    public string ServerResources { get; set; } = string.Empty;

    [Required(ErrorMessage = "Web Server is required."), StringLength(100)]
    public string WebServer { get; set; } = string.Empty;

    [StringLength(20)]
    public string DatabaseNameType { get; set; } = "none";

    [StringLength(200)]
    public string DatabaseName { get; set; } = string.Empty;

    [StringLength(200)]
    public string DatabaseUsername { get; set; } = string.Empty;

    [Required(ErrorMessage = "Hostname is required."), StringLength(200)]
    public string Hostname { get; set; } = string.Empty;

    public List<ServiceEntryDto> Services { get; set; } = new();
}
