using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VMWorkflow.Application.DTOs;
using VMWorkflow.Application.Interfaces;

namespace VMWorkflow.API.Controllers;

[ApiController]
[Route("api/requests/{id:guid}/soc")]
[Authorize(Roles = "SOC,IOCManager,PlatformAdmin")]
public class SOCController : ControllerBase
{
    private readonly IRequestService _requestService;

    public SOCController(IRequestService requestService)
    {
        _requestService = requestService;
    }

    [HttpPost]
    public async Task<ActionResult<RequestResponseDto>> SubmitSOC(Guid id, [FromBody] SOCDetailsDto dto, [FromQuery] string? action = null)
    {
        var user = User.Identity?.Name ?? throw new UnauthorizedAccessException("User identity not available.");
        var result = action?.ToLower() == "save"
            ? await _requestService.SaveSOCAsync(id, dto, user)
            : await _requestService.SubmitSOCAsync(id, dto, user);
        return Ok(result);
    }
}
