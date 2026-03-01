using VMWorkflow.Web.Models;

namespace VMWorkflow.Web.Services;

public interface IApiClient
{
    // Requests
    Task<List<RequestResponse>> GetRequestsAsync();
    Task<RequestResponse?> GetRequestAsync(Guid requestId);
    Task<RequestResponse> CreateRequestAsync(CreateRequestModel model);
    Task<RequestResponse> UpdateRequestAsync(Guid requestId, UpdateRequestModel model);
    Task<RequestResponse> SubmitRequestAsync(Guid requestId);
    Task<RequestResponse> SubmitSysAdminAsync(Guid requestId, SysAdminDetailsModel model);
    Task<RequestResponse> SubmitDataCenterAsync(Guid requestId, DataCenterDetailsModel model);
    Task<RequestResponse> SubmitNOCAsync(Guid requestId, NOCDetailsModel model);
    Task<RequestResponse> SubmitSOCAsync(Guid requestId, SOCDetailsModel model);
    Task<RequestResponse> ProcessApprovalAsync(Guid requestId, ApprovalModel model);
    Task<RequestResponse> ProcessCISOApprovalAsync(Guid requestId, ApprovalModel model);
    Task<RequestResponse> ProcessCTOApprovalAsync(Guid requestId, ApprovalModel model);
    Task<RequestResponse> ProcessOpsApprovalAsync(Guid requestId, ApprovalModel model);
    Task<RequestResponse> SendBackAsync(Guid requestId, SendBackModel model);

    // Audit Logs
    Task<List<AuditLogEntry>> GetAuditLogsAsync(DateTime? from = null, DateTime? to = null, string? user = null);
    Task<List<StatusHistoryLogEntry>> GetStatusHistoryLogsAsync(DateTime? from = null, DateTime? to = null, string? user = null);

    // Users
    Task<List<UserModel>> GetUsersAsync();
    Task<UserModel> CreateUserAsync(CreateUserModel model);
    Task<UserModel> UpdateUserRoleAsync(Guid userId, string role);
    Task<UserModel> ToggleBlockAsync(Guid userId, bool isBlocked);
}
