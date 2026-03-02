using System.ComponentModel.DataAnnotations;

namespace VMWorkflow.Application.DTOs;

public class SendBackDto
{
    [Required, StringLength(5000, MinimumLength = 1)]
    public string Comments { get; set; } = string.Empty;
}
