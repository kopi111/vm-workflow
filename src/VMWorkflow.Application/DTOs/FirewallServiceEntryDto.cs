using System.ComponentModel.DataAnnotations;

namespace VMWorkflow.Application.DTOs;

public class FirewallServiceEntryDto
{
    [Required, StringLength(20)]
    public string Port { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string Protocol { get; set; } = string.Empty;

    [StringLength(200)]
    public string? ServiceName { get; set; }
}
