using System.ComponentModel.DataAnnotations;

namespace VMWorkflow.Application.DTOs;

public class SendBackDto
{
    [Required(ErrorMessage = "Please add a comment."), StringLength(5000, MinimumLength = 1, ErrorMessage = "Please add a comment.")]
    public string Comments { get; set; } = string.Empty;

    /// <summary>
    /// Optional target status for send-back (e.g. "PendingNOC" or "PendingSOC").
    /// If null, falls back to the default previous status.
    /// </summary>
    public string? TargetStatus { get; set; }
}
