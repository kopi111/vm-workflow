using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VMWorkflow.Application.DTOs;
using VMWorkflow.Application.Interfaces;

namespace VMWorkflow.API.Controllers;

[ApiController]
[Route("api/requests/{id:guid}/noc")]
[Authorize(Roles = "NOC,IOCManager,PlatformAdmin")]
public class NOCController : ControllerBase
{
    private readonly IRequestService _requestService;

    public NOCController(IRequestService requestService)
    {
        _requestService = requestService;
    }

    [HttpPost]
    public async Task<ActionResult<RequestResponseDto>> SubmitNOC(Guid id, [FromBody] NOCDetailsDto dto, [FromQuery] string? action = null)
    {
        var user = User.Identity?.Name ?? throw new UnauthorizedAccessException("User identity not available.");
        var result = action?.ToLower() == "save"
            ? await _requestService.SaveNOCAsync(id, dto, user)
            : await _requestService.SubmitNOCAsync(id, dto, user);
        return Ok(result);
    }
}
