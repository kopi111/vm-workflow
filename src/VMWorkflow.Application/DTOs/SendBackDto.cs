using System.ComponentModel.DataAnnotations;

namespace VMWorkflow.Application.DTOs;

public class SendBackDto
{
    [Required]
    public string Comments { get; set; } = string.Empty;
}
