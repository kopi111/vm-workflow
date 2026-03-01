using Microsoft.AspNetCore.Mvc;
using VMWorkflow.Application.DTOs;
using VMWorkflow.Application.Interfaces;

namespace VMWorkflow.API.Controllers;

[ApiController]
[Route("api/requests/{id:guid}/approve")]
public class ApprovalController : ControllerBase
{
    private readonly IRequestService _requestService;

    public ApprovalController(IRequestService requestService)
    {
        _requestService = requestService;
    }

    /// <summary>
    /// Generic approval endpoint. Use role-specific endpoints instead:
    /// POST /api/requests/{id}/ciso-approve
    /// POST /api/requests/{id}/cto-approve
    /// POST /api/requests/{id}/ops-approve
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<RequestResponseDto>> ProcessApproval(Guid id, [FromBody] ApprovalDto dto, [FromQuery] string role = "ciso")
    {
        var user = User.Identity?.Name ?? "dev-user";
        var result = await _requestService.ProcessApprovalAsync(id, dto, user, role);
        return Ok(result);
    }
}
