using System.ComponentModel.DataAnnotations;

namespace VMWorkflow.Application.DTOs;

public class IOCSubmitDto
{
    [StringLength(5000)]
    public string? Comments { get; set; }
}
