using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VMWorkflow.Application.DTOs;
using VMWorkflow.Application.Interfaces;

namespace VMWorkflow.API.Controllers;

[Route("api/requests/{id:guid}/ciso-approve")]
[Authorize(Roles = "CISO,PlatformAdmin")]
public class CISOApprovalController : ApiControllerBase
{
    private readonly IRequestService _requestService;

    public CISOApprovalController(IRequestService requestService)
    {
        _requestService = requestService;
    }

    [HttpPost]
    public async Task<ActionResult<RequestResponseDto>> ProcessCISOApproval(Guid id, [FromBody] ApprovalDto dto)
    {
        var user = RequireAuthenticatedUsername();
        var result = await _requestService.ProcessApprovalAsync(id, dto, user, "ciso");
        return Ok(result);
    }
}
