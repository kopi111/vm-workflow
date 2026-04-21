using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VMWorkflow.Application.DTOs;
using VMWorkflow.Application.Interfaces;

namespace VMWorkflow.API.Controllers;

[Route("api/requests/{id:guid}/ops-approve")]
[Authorize(Roles = "Ops,PlatformAdmin")]
public class OpsManagerApprovalController : ApiControllerBase
{
    private readonly IRequestService _requestService;

    public OpsManagerApprovalController(IRequestService requestService)
    {
        _requestService = requestService;
    }

    [HttpPost]
    public async Task<ActionResult<RequestResponseDto>> ProcessOpsApproval(Guid id, [FromBody] ApprovalDto dto)
    {
        var user = RequireAuthenticatedUsername();
        var result = await _requestService.ProcessApprovalAsync(id, dto, user, "ops");
        return Ok(result);
    }
}
