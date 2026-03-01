using VMWorkflow.Domain.Enums;

namespace VMWorkflow.Application.Interfaces;

public interface IWorkflowEngine
{
    bool CanTransition(RequestStatus currentStatus, RequestStatus targetStatus);
    RequestStatus GetNextStatusAfterSysAdmin(RequestStatus currentStatus);
    RequestStatus GetNextStatusAfterDataCenter(RequestStatus currentStatus);
    RequestStatus GetNextStatusAfterNOC(RequestStatus currentStatus);
    RequestStatus GetNextStatusAfterSOC(RequestStatus currentStatus);
    bool IsIOCReady(bool nocCompleted, bool socCompleted);
    bool HasQuorumApproval(string? cisoDecision, string? ctoDecision, string? opsDecision);
    bool HasRejection(string? cisoDecision, string? ctoDecision, string? opsDecision);
    RequestStatus GetPreviousStatus(RequestStatus currentStatus);
}
