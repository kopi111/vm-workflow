using Microsoft.AspNetCore.Mvc;
using VMWorkflow.Application.DTOs;
using VMWorkflow.Application.Interfaces;

namespace VMWorkflow.API.Controllers;

[ApiController]
[Route("api/requests/{id:guid}/cto-approve")]
public class CTOApprovalController : ControllerBase
{
    private readonly IRequestService _requestService;

    public CTOApprovalController(IRequestService requestService)
    {
        _requestService = requestService;
    }

    [HttpPost]
    public async Task<ActionResult<RequestResponseDto>> ProcessCTOApproval(Guid id, [FromBody] ApprovalDto dto)
    {
        var user = User.Identity?.Name ?? "dev-user";
        var result = await _requestService.ProcessApprovalAsync(id, dto, user, "cto");
        return Ok(result);
    }
}
