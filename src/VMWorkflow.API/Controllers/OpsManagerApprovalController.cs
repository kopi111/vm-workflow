using Microsoft.AspNetCore.Mvc;
using VMWorkflow.Application.DTOs;
using VMWorkflow.Application.Interfaces;

namespace VMWorkflow.API.Controllers;

[ApiController]
[Route("api/requests/{id:guid}/ops-approve")]
public class OpsManagerApprovalController : ControllerBase
{
    private readonly IRequestService _requestService;

    public OpsManagerApprovalController(IRequestService requestService)
    {
        _requestService = requestService;
    }

    [HttpPost]
    public async Task<ActionResult<RequestResponseDto>> ProcessOpsApproval(Guid id, [FromBody] ApprovalDto dto)
    {
        var user = User.Identity?.Name ?? "dev-user";
        var result = await _requestService.ProcessApprovalAsync(id, dto, user, "ops");
        return Ok(result);
    }
}
