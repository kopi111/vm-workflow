using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VMWorkflow.Application.DTOs;
using VMWorkflow.Application.Interfaces;

namespace VMWorkflow.API.Controllers;

[Route("api/requests/{id:guid}/approve")]
[Authorize(Roles = "CISO,Ops,PlatformAdmin")]
public class ApprovalController : ApiControllerBase
{
    private readonly IRequestService _requestService;

    public ApprovalController(IRequestService requestService)
    {
        _requestService = requestService;
    }

    // Derives role from authenticated user's claims. Prefer /ciso-approve or /ops-approve.
    [HttpPost]
    public async Task<ActionResult<RequestResponseDto>> ProcessApproval(Guid id, [FromBody] ApprovalDto dto)
    {
        var user = RequireAuthenticatedUsername();
        var role = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value?.ToLower() ?? "ciso";
        var result = await _requestService.ProcessApprovalAsync(id, dto, user, role);
        return Ok(result);
    }
}
