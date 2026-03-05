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
    Task<RequestResponse> SaveSysAdminAsync(Guid requestId, SysAdminDetailsModel model);
    Task<RequestResponse> SubmitDataCenterAsync(Guid requestId, DataCenterDetailsModel model);
    Task<RequestResponse> SaveDataCenterAsync(Guid requestId, DataCenterDetailsModel model);
    Task<RequestResponse> SubmitNOCAsync(Guid requestId, NOCDetailsModel model);
    Task<RequestResponse> SaveNOCAsync(Guid requestId, NOCDetailsModel model);
    Task<RequestResponse> SubmitSOCAsync(Guid requestId, SOCDetailsModel model);
    Task<RequestResponse> SaveSOCAsync(Guid requestId, SOCDetailsModel model);
    Task<RequestResponse> ProcessApprovalAsync(Guid requestId, ApprovalModel model);
    Task<RequestResponse> ProcessIOCSubmitAsync(Guid requestId, string? comments);
    Task<RequestResponse> ProcessCISOApprovalAsync(Guid requestId, ApprovalModel model);
    Task<RequestResponse> ProcessOpsApprovalAsync(Guid requestId, ApprovalModel model);
    Task<RequestResponse> SendBackAsync(Guid requestId, SendBackModel model);
    Task<RequestResponse> IOCSendBackAsync(Guid requestId, SendBackModel model);
    Task<RequestResponse> RejectRequestAsync(Guid requestId, SendBackModel model);
    Task<RequestResponse> UnrejectRequestAsync(Guid requestId);
    Task<List<RequestResponse>> GetRejectedRequestsAsync();
    Task DeleteRequestAsync(Guid requestId);

    // Audit Logs
    Task<List<AuditLogEntry>> GetAuditLogsAsync(DateTime? from = null, DateTime? to = null, string? user = null);
    Task<List<StatusHistoryLogEntry>> GetStatusHistoryLogsAsync(DateTime? from = null, DateTime? to = null, string? user = null);

    // Work Queue
    Task<List<RequestResponse>> GetQueueAsync(string role);
    Task<List<RequestResponse>> GetSentBackRequestsAsync();

    // Resource Groups
    Task<List<ResourceGroupModel>> GetResourceGroupsAsync();
    Task<ResourceGroupModel> CreateResourceGroupAsync(ResourceGroupModel model);
    Task UpdateResourceGroupAsync(Guid id, ResourceGroupModel model);
    Task DeleteResourceGroupAsync(Guid id);

    // Security Profiles
    Task<List<SecurityProfileModel>> GetSecurityProfilesAsync();
    Task<SecurityProfileModel> CreateSecurityProfileAsync(SecurityProfileModel model);
    Task UpdateSecurityProfileAsync(Guid id, SecurityProfileModel model);
    Task DeleteSecurityProfileAsync(Guid id);

    // VDOMs
    Task<List<VdomModel>> GetVdomsAsync();
    Task<VdomModel> CreateVdomAsync(VdomModel model);
    Task UpdateVdomAsync(Guid id, VdomModel model);
    Task DeleteVdomAsync(Guid id);

    // Dropdown Options
    Task<List<DropdownOptionModel>> GetDropdownOptionsAsync(string category);
    Task<DropdownOptionModel> CreateDropdownOptionAsync(DropdownOptionModel model);
    Task UpdateDropdownOptionAsync(Guid id, DropdownOptionModel model);
    Task DeleteDropdownOptionAsync(Guid id);

    // Scripts
    Task<List<ScriptModel>> GetScriptsAsync();
    Task<byte[]> DownloadScriptAsync(Guid scriptId);

    // Users
    Task<List<UserModel>> GetUsersAsync();
    Task<UserModel> CreateUserAsync(CreateUserModel model);
    Task<UserModel> UpdateUserRoleAsync(Guid userId, string role);
    Task<UserModel> ToggleBlockAsync(Guid userId, bool isBlocked);
}
