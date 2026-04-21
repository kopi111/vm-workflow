using System.ComponentModel.DataAnnotations;

namespace VMWorkflow.Application.DTOs;

public class SysAdminDetailsDto
{
    [Required(ErrorMessage = "Sensitivity Level is required."), StringLength(30)]
    public string SensitivityLevel { get; set; } = string.Empty;

    [StringLength(50)]
    public string ServerResources { get; set; } = string.Empty;

    [Required(ErrorMessage = "Web Server is required."), StringLength(50)]
    public string WebServer { get; set; } = string.Empty;

    [StringLength(20)]
    public string DatabaseNameType { get; set; } = "none";

    [StringLength(64)]
    public string DatabaseName { get; set; } = string.Empty;

    [StringLength(64)]
    public string DatabaseUsername { get; set; } = string.Empty;

    [Required(ErrorMessage = "Hostname is required."), StringLength(64)]
    public string Hostname { get; set; } = string.Empty;

    public List<ServiceEntryDto> Services { get; set; } = new();
}
