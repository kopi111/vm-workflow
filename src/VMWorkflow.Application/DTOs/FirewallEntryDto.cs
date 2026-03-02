using System.ComponentModel.DataAnnotations;

namespace VMWorkflow.Application.DTOs;

public class FirewallEntryDto
{
    [Required, StringLength(200)]
    public string PolicyName { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string VDOM { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string SourceInterface { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string DestinationInterface { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string SourceIP { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string DestinationIP { get; set; } = string.Empty;

    public List<FirewallServiceEntryDto> Services { get; set; } = new();

    [Required, StringLength(100)]
    public string Schedule { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string Action { get; set; } = string.Empty;

    public List<Guid> SecurityProfileIds { get; set; } = new();

    public List<string> SecurityProfileNames { get; set; } = new();
}
