using System.ComponentModel.DataAnnotations;

namespace VMWorkflow.Application.DTOs;

public class ServiceEntryDto
{
    [StringLength(50)]
    public string ServiceName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Port is required."), StringLength(5)]
    public string Port { get; set; } = string.Empty;

    [Required(ErrorMessage = "Protocol is required."), StringLength(10)]
    public string Protocol { get; set; } = string.Empty;
}
