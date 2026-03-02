using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VMWorkflow.Application.DTOs;
using VMWorkflow.Application.Interfaces;

namespace VMWorkflow.API.Controllers;

[ApiController]
[Route("api/requests/{id:guid}/sysadmin")]
[Authorize(Roles = "SysAdmin,PlatformAdmin")]
public class SysAdminController : ControllerBase
{
    private readonly IRequestService _requestService;

    public SysAdminController(IRequestService requestService)
    {
        _requestService = requestService;
    }

    [HttpPost]
    public async Task<ActionResult<RequestResponseDto>> SubmitSysAdmin(Guid id, [FromBody] SysAdminDetailsDto dto, [FromQuery] string? action = null)
    {
        var user = User.Identity?.Name ?? throw new UnauthorizedAccessException("User identity not available.");
        var result = action?.ToLower() == "save"
            ? await _requestService.SaveSysAdminAsync(id, dto, user)
            : await _requestService.SubmitSysAdminAsync(id, dto, user);
        return Ok(result);
    }
}
