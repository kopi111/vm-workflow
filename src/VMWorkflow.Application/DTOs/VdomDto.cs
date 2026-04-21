using System.ComponentModel.DataAnnotations;

namespace VMWorkflow.Application.DTOs;

public class VdomDto
{
    public Guid? VdomId { get; set; }

    [Required]
    [StringLength(50, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;
}
