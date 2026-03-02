using System.ComponentModel.DataAnnotations;

namespace VMWorkflow.Application.DTOs;

public class ApprovalDto
{
    public ApprovalDecision Decision { get; set; }

    [StringLength(5000)]
    public string? Comments { get; set; }
}

public enum ApprovalDecision
{
    Approve,
    Reject,
    Return
}
