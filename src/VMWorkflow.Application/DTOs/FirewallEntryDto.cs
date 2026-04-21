using System.ComponentModel.DataAnnotations;

namespace VMWorkflow.Application.DTOs;

public class FirewallEntryDto
{
    [Required(ErrorMessage = "Policy Name is required."), StringLength(100)]
    public string PolicyName { get; set; } = string.Empty;

    [Required(ErrorMessage = "VDOM is required."), StringLength(50)]
    public string VDOM { get; set; } = string.Empty;

    [Required(ErrorMessage = "Source Interface is required."), StringLength(50)]
    public string SourceInterface { get; set; } = string.Empty;

    [Required(ErrorMessage = "Destination Interface is required."), StringLength(50)]
    public string DestinationInterface { get; set; } = string.Empty;

    [Required(ErrorMessage = "Source IP is required."), StringLength(50)]
    public string SourceIP { get; set; } = string.Empty;

    [Required(ErrorMessage = "Destination IP is required."), StringLength(50)]
    public string DestinationIP { get; set; } = string.Empty;

    public List<FirewallServiceEntryDto> Services { get; set; } = new();

    [Required(ErrorMessage = "Schedule is required."), StringLength(50)]
    public string Schedule { get; set; } = string.Empty;

    [Required(ErrorMessage = "Action is required."), StringLength(10)]
    public string Action { get; set; } = string.Empty;

    public List<Guid> SecurityProfileIds { get; set; } = new();

    public List<string> SecurityProfileNames { get; set; } = new();
}
