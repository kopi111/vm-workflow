using System.ComponentModel.DataAnnotations;

namespace VMWorkflow.Application.DTOs;

public class ServiceEntryDto
{
    [StringLength(200)]
    public string ServiceName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Port is required."), StringLength(20)]
    public string Port { get; set; } = string.Empty;

    [Required(ErrorMessage = "Protocol is required."), StringLength(20)]
    public string Protocol { get; set; } = string.Empty;
}
