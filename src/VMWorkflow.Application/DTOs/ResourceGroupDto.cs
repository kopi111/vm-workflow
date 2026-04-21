using System.ComponentModel.DataAnnotations;

namespace VMWorkflow.Application.DTOs;

public class ResourceGroupDto
{
    public Guid? ResourceGroupId { get; set; }

    [Required]
    [StringLength(50, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    [Range(1, 1024)]
    public int VCpu { get; set; }

    [Required]
    [StringLength(20, MinimumLength = 1)]
    public string Ram { get; set; } = string.Empty;

    [Required]
    [StringLength(20, MinimumLength = 1)]
    public string Hdd { get; set; } = string.Empty;
}
