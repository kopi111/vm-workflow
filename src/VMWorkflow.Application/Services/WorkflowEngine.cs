using VMWorkflow.Application.Interfaces;
using VMWorkflow.Domain.Enums;

namespace VMWorkflow.Application.Services;

public class WorkflowEngine : IWorkflowEngine
{
    private static readonly Dictionary<RequestStatus, HashSet<RequestStatus>> _validTransitions = new()
    {
        [RequestStatus.Draft] = new() { RequestStatus.PendingSysAdmin },
        [RequestStatus.PendingSysAdmin] = new() { RequestStatus.DataCenterReview, RequestStatus.Draft },
        [RequestStatus.DataCenterReview] = new() { RequestStatus.PendingNOC, RequestStatus.PendingSOC, RequestStatus.PendingSysAdmin },
        [RequestStatus.PendingNOC] = new() { RequestStatus.PendingSOC, RequestStatus.PendingIOCApproval, RequestStatus.DataCenterReview },
        [RequestStatus.PendingSOC] = new() { RequestStatus.PendingNOC, RequestStatus.PendingIOCApproval, RequestStatus.DataCenterReview },
        [RequestStatus.PendingIOCApproval] = new() { RequestStatus.PendingApproval, RequestStatus.Rejected, RequestStatus.Draft, RequestStatus.PendingNOC, RequestStatus.PendingSOC },
        [RequestStatus.PendingApproval] = new() { RequestStatus.Approved, RequestStatus.Rejected, RequestStatus.Draft, RequestStatus.PendingIOCApproval },
        [RequestStatus.Approved] = new() { RequestStatus.Implemented },
        [RequestStatus.Implemented] = new() { RequestStatus.Closed },
        [RequestStatus.Rejected] = new() { RequestStatus.Draft },
        [RequestStatus.Closed] = new()
    };

    public bool CanTransition(RequestStatus currentStatus, RequestStatus targetStatus)
    {
        return _validTransitions.TryGetValue(currentStatus, out var allowed) && allowed.Contains(targetStatus);
    }

    public RequestStatus GetNextStatusAfterSysAdmin(RequestStatus currentStatus)
    {
        if (currentStatus != RequestStatus.PendingSysAdmin)
            throw new InvalidOperationException($"Cannot submit SysAdmin details when status is {currentStatus}. Expected: PendingSysAdmin.");

        return RequestStatus.DataCenterReview;
    }

    public RequestStatus GetNextStatusAfterDataCenter(RequestStatus currentStatus)
    {
        if (currentStatus != RequestStatus.DataCenterReview)
            throw new InvalidOperationException($"Cannot submit DataCenter details when status is {currentStatus}. Expected: DataCenterReview.");

        return RequestStatus.PendingNOC;
    }

    public RequestStatus GetNextStatusAfterNOC(RequestStatus currentStatus)
    {
        if (currentStatus != RequestStatus.PendingNOC && currentStatus != RequestStatus.PendingSOC)
            throw new InvalidOperationException($"Cannot submit NOC details when status is {currentStatus}. Expected: PendingNOC or PendingSOC.");

        return currentStatus;
    }

    public RequestStatus GetNextStatusAfterSOC(RequestStatus currentStatus)
    {
        if (currentStatus != RequestStatus.PendingSOC && currentStatus != RequestStatus.PendingNOC)
            throw new InvalidOperationException($"Cannot submit SOC details when status is {currentStatus}. Expected: PendingSOC or PendingNOC.");

        return currentStatus;
    }

    public bool IsIOCReady(bool nocCompleted, bool socCompleted)
    {
        return nocCompleted && socCompleted;
    }

    public bool HasFullApproval(string? cisoDecision, string? opsDecision)
    {
        return cisoDecision == "Approved" && opsDecision == "Approved";
    }

    public bool HasRejection(string? cisoDecision, string? opsDecision)
    {
        return new[] { cisoDecision, opsDecision }
            .Any(d => d == "Rejected");
    }

    private static readonly Dictionary<RequestStatus, RequestStatus> _previousStatus = new()
    {
        [RequestStatus.PendingSysAdmin] = RequestStatus.Draft,
        [RequestStatus.DataCenterReview] = RequestStatus.PendingSysAdmin,
        [RequestStatus.PendingNOC] = RequestStatus.DataCenterReview,
        [RequestStatus.PendingSOC] = RequestStatus.DataCenterReview,
        [RequestStatus.PendingIOCApproval] = RequestStatus.PendingNOC,
        [RequestStatus.PendingApproval] = RequestStatus.PendingIOCApproval
    };

    public RequestStatus GetPreviousStatus(RequestStatus currentStatus)
    {
        if (!_previousStatus.TryGetValue(currentStatus, out var previous))
            throw new InvalidOperationException($"Cannot send back from status {currentStatus}.");

        return previous;
    }
}
