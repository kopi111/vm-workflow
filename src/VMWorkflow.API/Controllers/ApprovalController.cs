using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VMWorkflow.Application.DTOs;
using VMWorkflow.Application.Interfaces;

namespace VMWorkflow.API.Controllers;

[ApiController]
[Route("api/requests/{id:guid}/approve")]
[Authorize(Roles = "CISO,CTO,Ops,PlatformAdmin")]
public class ApprovalController : ControllerBase
{
    private readonly IRequestService _requestService;

    public ApprovalController(IRequestService requestService)
    {
        _requestService = requestService;
    }

    /// <summary>
    /// Generic approval endpoint. Derives role from authenticated user's claims.
    /// Prefer role-specific endpoints: /ciso-approve, /cto-approve, /ops-approve
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<RequestResponseDto>> ProcessApproval(Guid id, [FromBody] ApprovalDto dto)
    {
        var user = User.Identity?.Name ?? throw new UnauthorizedAccessException("User identity not available.");
        var role = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value?.ToLower() ?? "ciso";
        var result = await _requestService.ProcessApprovalAsync(id, dto, user, role);
        return Ok(result);
    }
}
