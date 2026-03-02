using System.ComponentModel.DataAnnotations;

namespace VMWorkflow.Application.DTOs;

public class NetworkPathEntryDto
{
    [Required, StringLength(200)]
    public string SwitchName { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string Port { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string LinkSpeed { get; set; } = string.Empty;
}
