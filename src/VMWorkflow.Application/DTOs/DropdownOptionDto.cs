using System.ComponentModel.DataAnnotations;

namespace VMWorkflow.Application.DTOs;

public class DropdownOptionDto
{
    public Guid? DropdownOptionId { get; set; }

    [Required]
    [StringLength(30, MinimumLength = 1)]
    public string Category { get; set; } = string.Empty;

    [Required]
    [StringLength(50, MinimumLength = 1)]
    public string Value { get; set; } = string.Empty;

    public int SortOrder { get; set; }
}
