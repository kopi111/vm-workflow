using System.ComponentModel.DataAnnotations;

namespace VMWorkflow.Application.DTOs;

public class NetworkPathEntryDto
{
    [Required(ErrorMessage = "Switch Name is required."), StringLength(200)]
    public string SwitchName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Port is required."), StringLength(50)]
    public string Port { get; set; } = string.Empty;

    [Required(ErrorMessage = "Link Speed is required."), StringLength(50)]
    public string LinkSpeed { get; set; } = string.Empty;
}
