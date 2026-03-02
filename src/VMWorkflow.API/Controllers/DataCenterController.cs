using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VMWorkflow.Application.DTOs;
using VMWorkflow.Application.Interfaces;

namespace VMWorkflow.API.Controllers;

[ApiController]
[Route("api/requests/{id:guid}/datacenter")]
[Authorize(Roles = "DataCenter,IOCManager,PlatformAdmin")]
public class DataCenterController : ControllerBase
{
    private readonly IRequestService _requestService;

    public DataCenterController(IRequestService requestService)
    {
        _requestService = requestService;
    }

    [HttpPost]
    public async Task<ActionResult<RequestResponseDto>> SubmitDataCenter(Guid id, [FromBody] DataCenterDetailsDto dto, [FromQuery] string? action = null)
    {
        var user = User.Identity?.Name ?? throw new UnauthorizedAccessException("User identity not available.");
        var result = action?.ToLower() == "save"
            ? await _requestService.SaveDataCenterAsync(id, dto, user)
            : await _requestService.SubmitDataCenterAsync(id, dto, user);
        return Ok(result);
    }
}
