using System.Net.Http.Json;
using VMWorkflow.Web.Models;

namespace VMWorkflow.Web.Services;

public class ApiClient : IApiClient
{
    private readonly HttpClient _http;

    public ApiClient(HttpClient http)
    {
        _http = http;
    }

    // Requests
    public async Task<List<RequestResponse>> GetRequestsAsync()
    {
        return await _http.GetFromJsonAsync<List<RequestResponse>>("api/requests") ?? new();
    }

    public async Task<RequestResponse?> GetRequestAsync(Guid requestId)
    {
        return await _http.GetFromJsonAsync<RequestResponse>($"api/requests/{requestId}");
    }

    public async Task<RequestResponse> CreateRequestAsync(CreateRequestModel model)
    {
        var response = await _http.PostAsJsonAsync("api/requests", model);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<RequestResponse>())!;
    }

    public async Task<RequestResponse> UpdateRequestAsync(Guid requestId, UpdateRequestModel model)
    {
        var response = await _http.PutAsJsonAsync($"api/requests/{requestId}", model);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<RequestResponse>())!;
    }

    public async Task<RequestResponse> SubmitRequestAsync(Guid requestId)
    {
        var response = await _http.PostAsync($"api/requests/{requestId}/submit", null);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<RequestResponse>())!;
    }

    public async Task<RequestResponse> SubmitSysAdminAsync(Guid requestId, SysAdminDetailsModel model)
    {
        var response = await _http.PostAsJsonAsync($"api/requests/{requestId}/sysadmin", model);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<RequestResponse>())!;
    }

    public async Task<RequestResponse> SaveSysAdminAsync(Guid requestId, SysAdminDetailsModel model)
    {
        var response = await _http.PostAsJsonAsync($"api/requests/{requestId}/sysadmin?action=save", model);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<RequestResponse>())!;
    }

    public async Task<RequestResponse> SubmitDataCenterAsync(Guid requestId, DataCenterDetailsModel model)
    {
        var response = await _http.PostAsJsonAsync($"api/requests/{requestId}/datacenter", model);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<RequestResponse>())!;
    }

    public async Task<RequestResponse> SaveDataCenterAsync(Guid requestId, DataCenterDetailsModel model)
    {
        var response = await _http.PostAsJsonAsync($"api/requests/{requestId}/datacenter?action=save", model);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<RequestResponse>())!;
    }

    public async Task<RequestResponse> SubmitNOCAsync(Guid requestId, NOCDetailsModel model)
    {
        var response = await _http.PostAsJsonAsync($"api/requests/{requestId}/noc", model);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<RequestResponse>())!;
    }

    public async Task<RequestResponse> SaveNOCAsync(Guid requestId, NOCDetailsModel model)
    {
        var response = await _http.PostAsJsonAsync($"api/requests/{requestId}/noc?action=save", model);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<RequestResponse>())!;
    }

    public async Task<RequestResponse> SubmitSOCAsync(Guid requestId, SOCDetailsModel model)
    {
        var response = await _http.PostAsJsonAsync($"api/requests/{requestId}/soc", model);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<RequestResponse>())!;
    }

    public async Task<RequestResponse> SaveSOCAsync(Guid requestId, SOCDetailsModel model)
    {
        var response = await _http.PostAsJsonAsync($"api/requests/{requestId}/soc?action=save", model);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<RequestResponse>())!;
    }

    public async Task<RequestResponse> ProcessApprovalAsync(Guid requestId, ApprovalModel model)
    {
        var response = await _http.PostAsJsonAsync($"api/requests/{requestId}/approve", model);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<RequestResponse>())!;
    }

    public async Task<RequestResponse> ProcessCISOApprovalAsync(Guid requestId, ApprovalModel model)
    {
        var response = await _http.PostAsJsonAsync($"api/requests/{requestId}/ciso-approve", model);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<RequestResponse>())!;
    }

    public async Task<RequestResponse> ProcessCTOApprovalAsync(Guid requestId, ApprovalModel model)
    {
        var response = await _http.PostAsJsonAsync($"api/requests/{requestId}/cto-approve", model);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<RequestResponse>())!;
    }

    public async Task<RequestResponse> ProcessOpsApprovalAsync(Guid requestId, ApprovalModel model)
    {
        var response = await _http.PostAsJsonAsync($"api/requests/{requestId}/ops-approve", model);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<RequestResponse>())!;
    }

    public async Task<RequestResponse> SendBackAsync(Guid requestId, SendBackModel model)
    {
        var response = await _http.PostAsJsonAsync($"api/requests/{requestId}/send-back", model);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<RequestResponse>())!;
    }

    // Work Queue
    public async Task<List<RequestResponse>> GetQueueAsync(string role)
    {
        return await _http.GetFromJsonAsync<List<RequestResponse>>($"api/queue/{role}") ?? new();
    }

    // Resource Groups
    public async Task<List<ResourceGroupModel>> GetResourceGroupsAsync()
    {
        return await _http.GetFromJsonAsync<List<ResourceGroupModel>>("api/admin/resource-groups") ?? new();
    }

    // Resource Groups CRUD
    public async Task<ResourceGroupModel> CreateResourceGroupAsync(ResourceGroupModel model)
    {
        var response = await _http.PostAsJsonAsync("api/admin/resource-groups", model);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ResourceGroupModel>())!;
    }

    public async Task UpdateResourceGroupAsync(Guid id, ResourceGroupModel model)
    {
        var response = await _http.PutAsJsonAsync($"api/admin/resource-groups/{id}", model);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteResourceGroupAsync(Guid id)
    {
        var response = await _http.DeleteAsync($"api/admin/resource-groups/{id}");
        response.EnsureSuccessStatusCode();
    }

    // Security Profiles
    public async Task<List<SecurityProfileModel>> GetSecurityProfilesAsync()
    {
        return await _http.GetFromJsonAsync<List<SecurityProfileModel>>("api/admin/security-profiles") ?? new();
    }

    public async Task<SecurityProfileModel> CreateSecurityProfileAsync(SecurityProfileModel model)
    {
        var response = await _http.PostAsJsonAsync("api/admin/security-profiles", model);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<SecurityProfileModel>())!;
    }

    public async Task UpdateSecurityProfileAsync(Guid id, SecurityProfileModel model)
    {
        var response = await _http.PutAsJsonAsync($"api/admin/security-profiles/{id}", model);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteSecurityProfileAsync(Guid id)
    {
        var response = await _http.DeleteAsync($"api/admin/security-profiles/{id}");
        response.EnsureSuccessStatusCode();
    }

    // VDOMs
    public async Task<List<VdomModel>> GetVdomsAsync()
    {
        return await _http.GetFromJsonAsync<List<VdomModel>>("api/admin/vdoms") ?? new();
    }

    public async Task<VdomModel> CreateVdomAsync(VdomModel model)
    {
        var response = await _http.PostAsJsonAsync("api/admin/vdoms", model);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<VdomModel>())!;
    }

    public async Task UpdateVdomAsync(Guid id, VdomModel model)
    {
        var response = await _http.PutAsJsonAsync($"api/admin/vdoms/{id}", model);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteVdomAsync(Guid id)
    {
        var response = await _http.DeleteAsync($"api/admin/vdoms/{id}");
        response.EnsureSuccessStatusCode();
    }

    // Audit Logs
    public async Task<List<AuditLogEntry>> GetAuditLogsAsync(DateTime? from = null, DateTime? to = null, string? user = null)
    {
        var query = BuildLogQuery("api/logs/audit", from, to, user);
        return await _http.GetFromJsonAsync<List<AuditLogEntry>>(query) ?? new();
    }

    public async Task<List<StatusHistoryLogEntry>> GetStatusHistoryLogsAsync(DateTime? from = null, DateTime? to = null, string? user = null)
    {
        var query = BuildLogQuery("api/logs/status-history", from, to, user);
        return await _http.GetFromJsonAsync<List<StatusHistoryLogEntry>>(query) ?? new();
    }

    // Users
    public async Task<List<UserModel>> GetUsersAsync()
    {
        return await _http.GetFromJsonAsync<List<UserModel>>("api/users") ?? new();
    }

    public async Task<UserModel> CreateUserAsync(CreateUserModel model)
    {
        var response = await _http.PostAsJsonAsync("api/users", model);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<UserModel>())!;
    }

    public async Task<UserModel> UpdateUserRoleAsync(Guid userId, string role)
    {
        var response = await _http.PutAsJsonAsync($"api/users/{userId}/role", new { role });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<UserModel>())!;
    }

    public async Task<UserModel> ToggleBlockAsync(Guid userId, bool isBlocked)
    {
        var response = await _http.PutAsJsonAsync($"api/users/{userId}/block", new { isBlocked });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<UserModel>())!;
    }

    public async Task<T?> GetAsync<T>(string url)
    {
        return await _http.GetFromJsonAsync<T>(url);
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest body)
    {
        var response = await _http.PostAsJsonAsync(url, body);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TResponse>();
    }

    private static string BuildLogQuery(string basePath, DateTime? from, DateTime? to, string? user)
    {
        var parts = new List<string>();
        if (from.HasValue) parts.Add($"from={from.Value:O}");
        if (to.HasValue) parts.Add($"to={to.Value:O}");
        if (!string.IsNullOrEmpty(user)) parts.Add($"user={Uri.EscapeDataString(user)}");
        return parts.Count > 0 ? $"{basePath}?{string.Join("&", parts)}" : basePath;
    }
}
