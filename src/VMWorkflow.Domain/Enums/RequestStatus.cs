namespace VMWorkflow.Domain.Enums;

public enum RequestStatus
{
    Draft,
    PendingSysAdmin,
    DataCenterReview,
    PendingNOC,
    PendingSOC,
    PendingIOCApproval,
    PendingApproval,
    Approved,
    Rejected,
    Implemented,
    Closed
}
