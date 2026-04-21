using System.Net.Http.Json;
using System.Text.Json;
using VMWorkflow.Web.Models;

namespace VMWorkflow.Web.Services;

public class ApiClient : IApiClient
{
    private readonly HttpClient _http;

    public ApiClient(HttpClient http)
    {
        _http = http;
    }

    private static async Task EnsureSuccessOrThrowValidation(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        try
        {
            var body = await response.Content.ReadAsStringAsync();
            if (!string.IsNullOrWhiteSpace(body))
            {
                var doc = JsonDocument.Parse(body);

                // ASP.NET ValidationProblemDetails: { "errors": { "Field": ["msg"] } }
                if (doc.RootElement.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
                {
                    var messages = new List<string>();
                    foreach (var field in errors.EnumerateObject())
                        foreach (var msg in field.Value.EnumerateArray())
                            messages.Add(msg.GetString() ?? "Invalid value.");
                    if (messages.Count > 0)
                        throw new HttpRequestException(string.Join("\n", messages));
                }

                // GlobalExceptionHandler format: { "error": "message" }
                if (doc.RootElement.TryGetProperty("error", out var error) && error.GetString() is string errorMsg)
                    throw new HttpRequestException(errorMsg);

                // ProblemDetails: { "detail": "..." } or { "title": "..." }
                if (doc.RootElement.TryGetProperty("detail", out var detail) && detail.GetString() is string detailMsg)
                    throw new HttpRequestException(detailMsg);
                if (doc.RootElement.TryGetProperty("title", out var title) && title.GetString() is string titleMsg)
                    throw new HttpRequestException(titleMsg);
            }
        }
        catch (HttpRequestException) { throw; }
        catch { /* failed to parse body */ }

        var friendlyMessage = response.StatusCode switch
        {
            System.Net.HttpStatusCode.BadRequest => "The request contains invalid data. Please check your inputs and try again.",
            System.Net.HttpStatusCode.Unauthorized => "Your session has expired. Please log in again.",
            System.Net.HttpStatusCode.Forbidden => "You do not have permission to perform this action.",
            System.Net.HttpStatusCode.NotFound => "The requested item was not found.",
            System.Net.HttpStatusCode.Conflict => "This action conflicts with the current state. Please refresh and try again.",
            System.Net.HttpStatusCode.InternalServerError => "An unexpected server error occurred. Please try again later.",
            _ => $"Something went wrong (Error {(int)response.StatusCode}). Please try again."
        };
        throw new HttpRequestException(friendlyMessage);
    }

    private async Task<T> GetWithFriendlyErrorAsync<T>(string url, T fallback)
    {
        var response = await _http.GetAsync(url);
        await EnsureSuccessOrThrowValidation(response);
        return await response.Content.ReadFromJsonAsync<T>() ?? fallback;
    }

    public async Task<List<RequestResponse>> GetRequestsAsync()
    {
        return await GetWithFriendlyErrorAsync("api/requests", new List<RequestResponse>());
    }

    public async Task<List<RequestResponse>> GetMyDraftsAsync()
    {
        return await GetWithFriendlyErrorAsync("api/requests/drafts/mine", new List<RequestResponse>());
    }

    public async Task<RequestResponse?> GetRequestAsync(Guid requestId)
    {
        return await GetWithFriendlyErrorAsync<RequestResponse?>($"api/requests/{requestId}", null);
    }

    public async Task<RequestResponse> CreateRequestAsync(CreateRequestModel model)
    {
        var response = await _http.PostAsJsonAsync("api/requests", model);
        await EnsureSuccessOrThrowValidation(response);
        return (await response.Content.ReadFromJsonAsync<RequestResponse>())!;
    }

    public async Task<RequestResponse> UpdateRequestAsync(Guid requestId, UpdateRequestModel model)
    {
        var response = await _http.PutAsJsonAsync($"api/requests/{requestId}", model);
        await EnsureSuccessOrThrowValidation(response);
        return (await response.Content.ReadFromJsonAsync<RequestResponse>())!;
    }

    public async Task<RequestResponse> SubmitRequestAsync(Guid requestId)
    {
        var response = await _http.PostAsync($"api/requests/{requestId}/submit", null);
        await EnsureSuccessOrThrowValidation(response);
        return (await response.Content.ReadFromJsonAsync<RequestResponse>())!;
    }

    public async Task<RequestResponse> SubmitSysAdminAsync(Guid requestId, SysAdminDetailsModel model)
    {
        var response = await _http.PostAsJsonAsync($"api/requests/{requestId}/sysadmin", model);
        await EnsureSuccessOrThrowValidation(response);
        return (await response.Content.ReadFromJsonAsync<RequestResponse>())!;
    }

    public async Task<RequestResponse> SaveSysAdminAsync(Guid requestId, SysAdminDetailsModel model)
    {
        var response = await _http.PostAsJsonAsync($"api/requests/{requestId}/sysadmin?action=save", model);
        await EnsureSuccessOrThrowValidation(response);
        return (await response.Content.ReadFromJsonAsync<RequestResponse>())!;
    }

    public async Task<RequestResponse> SubmitDataCenterAsync(Guid requestId, DataCenterDetailsModel model)
    {
        var response = await _http.PostAsJsonAsync($"api/requests/{requestId}/datacenter", model);
        await EnsureSuccessOrThrowValidation(response);
        return (await response.Content.ReadFromJsonAsync<RequestResponse>())!;
    }

    public async Task<RequestResponse> SaveDataCenterAsync(Guid requestId, DataCenterDetailsModel model)
    {
        var response = await _http.PostAsJsonAsync($"api/requests/{requestId}/datacenter?action=save", model);
        await EnsureSuccessOrThrowValidation(response);
        return (await response.Content.ReadFromJsonAsync<RequestResponse>())!;
    }

    public async Task<RequestResponse> SubmitNOCAsync(Guid requestId, NOCDetailsModel model)
    {
        var response = await _http.PostAsJsonAsync($"api/requests/{requestId}/noc", model);
        await EnsureSuccessOrThrowValidation(response);
        return (await response.Content.ReadFromJsonAsync<RequestResponse>())!;
    }

    public async Task<RequestResponse> SaveNOCAsync(Guid requestId, NOCDetailsModel model)
    {
        var response = await _http.PostAsJsonAsync($"api/requests/{requestId}/noc?action=save", model);
        await EnsureSuccessOrThrowValidation(response);
        return (await response.Content.ReadFromJsonAsync<RequestResponse>())!;
    }

    public async Task<RequestResponse> SubmitSOCAsync(Guid requestId, SOCDetailsModel model)
    {
        var response = await _http.PostAsJsonAsync($"api/requests/{requestId}/soc", model);
        await EnsureSuccessOrThrowValidation(response);
        return (await response.Content.ReadFromJsonAsync<RequestResponse>())!;
    }

    public async Task<RequestResponse> SaveSOCAsync(Guid requestId, SOCDetailsModel model)
    {
        var response = await _http.PostAsJsonAsync($"api/requests/{requestId}/soc?action=save", model);
        await EnsureSuccessOrThrowValidation(response);
        return (await response.Content.ReadFromJsonAsync<RequestResponse>())!;
    }

    public async Task<RequestResponse> ProcessApprovalAsync(Guid requestId, ApprovalModel model)
    {
        var response = await _http.PostAsJsonAsync($"api/requests/{requestId}/approve", model);
        await EnsureSuccessOrThrowValidation(response);
        return (await response.Content.ReadFromJsonAsync<RequestResponse>())!;
    }

    public async Task<RequestResponse> ProcessIOCSubmitAsync(Guid requestId, string? comments)
    {
        var response = await _http.PostAsJsonAsync($"api/requests/{requestId}/ioc/submit", new { Comments = comments });
        await EnsureSuccessOrThrowValidation(response);
        return (await response.Content.ReadFromJsonAsync<RequestResponse>())!;
    }

    public async Task<RequestResponse> ProcessCISOApprovalAsync(Guid requestId, ApprovalModel model)
    {
        var response = await _http.PostAsJsonAsync($"api/requests/{requestId}/ciso-approve", model);
        await EnsureSuccessOrThrowValidation(response);
        return (await response.Content.ReadFromJsonAsync<RequestResponse>())!;
    }

    public async Task<RequestResponse> ProcessOpsApprovalAsync(Guid requestId, ApprovalModel model)
    {
        var response = await _http.PostAsJsonAsync($"api/requests/{requestId}/ops-approve", model);
        await EnsureSuccessOrThrowValidation(response);
        return (await response.Content.ReadFromJsonAsync<RequestResponse>())!;
    }

    public async Task<RequestResponse> SendBackAsync(Guid requestId, SendBackModel model)
    {
        var response = await _http.PostAsJsonAsync($"api/requests/{requestId}/send-back", model);
        await EnsureSuccessOrThrowValidation(response);
        return (await response.Content.ReadFromJsonAsync<RequestResponse>())!;
    }

    public async Task<RequestResponse> IOCSendBackAsync(Guid requestId, SendBackModel model)
    {
        var response = await _http.PostAsJsonAsync($"api/requests/{requestId}/ioc/send-back", model);
        await EnsureSuccessOrThrowValidation(response);
        return (await response.Content.ReadFromJsonAsync<RequestResponse>())!;
    }

    public async Task<RequestResponse> RejectRequestAsync(Guid requestId, SendBackModel model)
    {
        var response = await _http.PostAsJsonAsync($"api/requests/{requestId}/ioc/reject", model);
        await EnsureSuccessOrThrowValidation(response);
        return (await response.Content.ReadFromJsonAsync<RequestResponse>())!;
    }

    public async Task<RequestResponse> UnrejectRequestAsync(Guid requestId)
    {
        var response = await _http.PostAsync($"api/requests/{requestId}/ioc/unreject", null);
        await EnsureSuccessOrThrowValidation(response);
        return (await response.Content.ReadFromJsonAsync<RequestResponse>())!;
    }

    public async Task<List<RequestResponse>> GetRejectedRequestsAsync()
    {
        return await GetWithFriendlyErrorAsync("api/queue/rejected", new List<RequestResponse>());
    }

    public async Task DeleteRequestAsync(Guid requestId)
    {
        var response = await _http.DeleteAsync($"api/requests/{requestId}");
        await EnsureSuccessOrThrowValidation(response);
    }

    public async Task<List<RequestResponse>> GetQueueAsync(string role)
    {
        return await GetWithFriendlyErrorAsync($"api/queue/{role}", new List<RequestResponse>());
    }

    public async Task<List<RequestResponse>> GetSentBackRequestsAsync()
    {
        return await GetWithFriendlyErrorAsync("api/queue/sent-back", new List<RequestResponse>());
    }

    public async Task<List<RequestResponse>> GetSentBackToMeAsync()
    {
        return await GetWithFriendlyErrorAsync("api/queue/sent-back-to-me", new List<RequestResponse>());
    }

    public async Task<List<ResourceGroupModel>> GetResourceGroupsAsync()
    {
        return await GetWithFriendlyErrorAsync("api/admin/resource-groups", new List<ResourceGroupModel>());
    }

    public async Task<ResourceGroupModel> CreateResourceGroupAsync(ResourceGroupModel model)
    {
        var response = await _http.PostAsJsonAsync("api/admin/resource-groups", model);
        await EnsureSuccessOrThrowValidation(response);
        return (await response.Content.ReadFromJsonAsync<ResourceGroupModel>())!;
    }

    public async Task UpdateResourceGroupAsync(Guid id, ResourceGroupModel model)
    {
        var response = await _http.PutAsJsonAsync($"api/admin/resource-groups/{id}", model);
        await EnsureSuccessOrThrowValidation(response);
    }

    public async Task DeleteResourceGroupAsync(Guid id)
    {
        var response = await _http.DeleteAsync($"api/admin/resource-groups/{id}");
        await EnsureSuccessOrThrowValidation(response);
    }

    public async Task<List<SecurityProfileModel>> GetSecurityProfilesAsync()
    {
        return await GetWithFriendlyErrorAsync("api/admin/security-profiles", new List<SecurityProfileModel>());
    }

    public async Task<SecurityProfileModel> CreateSecurityProfileAsync(SecurityProfileModel model)
    {
        var response = await _http.PostAsJsonAsync("api/admin/security-profiles", model);
        await EnsureSuccessOrThrowValidation(response);
        return (await response.Content.ReadFromJsonAsync<SecurityProfileModel>())!;
    }

    public async Task UpdateSecurityProfileAsync(Guid id, SecurityProfileModel model)
    {
        var response = await _http.PutAsJsonAsync($"api/admin/security-profiles/{id}", model);
        await EnsureSuccessOrThrowValidation(response);
    }

    public async Task DeleteSecurityProfileAsync(Guid id)
    {
        var response = await _http.DeleteAsync($"api/admin/security-profiles/{id}");
        await EnsureSuccessOrThrowValidation(response);
    }

    public async Task<List<VdomModel>> GetVdomsAsync()
    {
        return await GetWithFriendlyErrorAsync("api/admin/vdoms", new List<VdomModel>());
    }

    public async Task<VdomModel> CreateVdomAsync(VdomModel model)
    {
        var response = await _http.PostAsJsonAsync("api/admin/vdoms", model);
        await EnsureSuccessOrThrowValidation(response);
        return (await response.Content.ReadFromJsonAsync<VdomModel>())!;
    }

    public async Task UpdateVdomAsync(Guid id, VdomModel model)
    {
        var response = await _http.PutAsJsonAsync($"api/admin/vdoms/{id}", model);
        await EnsureSuccessOrThrowValidation(response);
    }

    public async Task DeleteVdomAsync(Guid id)
    {
        var response = await _http.DeleteAsync($"api/admin/vdoms/{id}");
        await EnsureSuccessOrThrowValidation(response);
    }

    public async Task<List<ScheduleModel>> GetSchedulesAsync()
    {
        return await GetWithFriendlyErrorAsync("api/admin/schedules", new List<ScheduleModel>());
    }

    public async Task<ScheduleModel> CreateScheduleAsync(ScheduleModel model)
    {
        var response = await _http.PostAsJsonAsync("api/admin/schedules", model);
        await EnsureSuccessOrThrowValidation(response);
        return (await response.Content.ReadFromJsonAsync<ScheduleModel>())!;
    }

    public async Task UpdateScheduleAsync(Guid id, ScheduleModel model)
    {
        var response = await _http.PutAsJsonAsync($"api/admin/schedules/{id}", model);
        await EnsureSuccessOrThrowValidation(response);
    }

    public async Task DeleteScheduleAsync(Guid id)
    {
        var response = await _http.DeleteAsync($"api/admin/schedules/{id}");
        await EnsureSuccessOrThrowValidation(response);
    }

    public async Task<List<DropdownOptionModel>> GetDropdownOptionsAsync(string category)
    {
        return await GetWithFriendlyErrorAsync($"api/admin/dropdown-options/{Uri.EscapeDataString(category)}", new List<DropdownOptionModel>());
    }

    public async Task<DropdownOptionModel> CreateDropdownOptionAsync(DropdownOptionModel model)
    {
        var response = await _http.PostAsJsonAsync("api/admin/dropdown-options", model);
        await EnsureSuccessOrThrowValidation(response);
        return (await response.Content.ReadFromJsonAsync<DropdownOptionModel>())!;
    }

    public async Task UpdateDropdownOptionAsync(Guid id, DropdownOptionModel model)
    {
        var response = await _http.PutAsJsonAsync($"api/admin/dropdown-options/{id}", model);
        await EnsureSuccessOrThrowValidation(response);
    }

    public async Task DeleteDropdownOptionAsync(Guid id)
    {
        var response = await _http.DeleteAsync($"api/admin/dropdown-options/{id}");
        await EnsureSuccessOrThrowValidation(response);
    }

    public async Task<List<AuditLogEntry>> GetAuditLogsAsync(DateTime? from = null, DateTime? to = null, string? user = null)
    {
        var query = BuildLogQuery("api/logs/audit", from, to, user);
        return await GetWithFriendlyErrorAsync(query, new List<AuditLogEntry>());
    }

    public async Task<List<StatusHistoryLogEntry>> GetStatusHistoryLogsAsync(DateTime? from = null, DateTime? to = null, string? user = null)
    {
        var query = BuildLogQuery("api/logs/status-history", from, to, user);
        return await GetWithFriendlyErrorAsync(query, new List<StatusHistoryLogEntry>());
    }

    public async Task<List<ScriptModel>> GetScriptsAsync()
    {
        return await GetWithFriendlyErrorAsync("api/scripts", new List<ScriptModel>());
    }

    public async Task<byte[]> DownloadScriptAsync(Guid scriptId)
    {
        var response = await _http.GetAsync($"api/scripts/{scriptId}/download");
        await EnsureSuccessOrThrowValidation(response);
        return await response.Content.ReadAsByteArrayAsync();
    }

    public async Task<List<UserModel>> GetUsersAsync()
    {
        return await GetWithFriendlyErrorAsync("api/users", new List<UserModel>());
    }

    public async Task<UserModel> CreateUserAsync(CreateUserModel model)
    {
        var response = await _http.PostAsJsonAsync("api/users", model);
        await EnsureSuccessOrThrowValidation(response);
        return (await response.Content.ReadFromJsonAsync<UserModel>())!;
    }

    public async Task<UserModel> UpdateUserRoleAsync(Guid userId, string role)
    {
        var response = await _http.PutAsJsonAsync($"api/users/{userId}/role", new { role });
        await EnsureSuccessOrThrowValidation(response);
        return (await response.Content.ReadFromJsonAsync<UserModel>())!;
    }

    public async Task<UserModel> ToggleBlockAsync(Guid userId, bool isBlocked)
    {
        var response = await _http.PutAsJsonAsync($"api/users/{userId}/block", new { isBlocked });
        await EnsureSuccessOrThrowValidation(response);
        return (await response.Content.ReadFromJsonAsync<UserModel>())!;
    }

    public async Task<T?> GetAsync<T>(string url)
    {
        return await GetWithFriendlyErrorAsync<T?>(url, default);
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest body)
    {
        var response = await _http.PostAsJsonAsync(url, body);
        await EnsureSuccessOrThrowValidation(response);
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
