using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VMWorkflow.Application.DTOs;
using VMWorkflow.Application.Interfaces;

namespace VMWorkflow.API.Controllers;

[ApiController]
[Route("api/requests/{id:guid}/ciso-approve")]
[Authorize(Roles = "CISO,PlatformAdmin")]
public class CISOApprovalController : ControllerBase
{
    private readonly IRequestService _requestService;

    public CISOApprovalController(IRequestService requestService)
    {
        _requestService = requestService;
    }

    [HttpPost]
    public async Task<ActionResult<RequestResponseDto>> ProcessCISOApproval(Guid id, [FromBody] ApprovalDto dto)
    {
        var user = User.Identity?.Name ?? throw new UnauthorizedAccessException("User identity not available.");
        var result = await _requestService.ProcessApprovalAsync(id, dto, user, "ciso");
        return Ok(result);
    }
}
