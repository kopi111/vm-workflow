namespace VMWorkflow.Application.DTOs;

public class ApprovalDto
{
    public ApprovalDecision Decision { get; set; }
    public string? Comments { get; set; }
}

public enum ApprovalDecision
{
    Approve,
    Reject,
    Return
}
