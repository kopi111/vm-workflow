using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VMWorkflow.Application.DTOs;
using VMWorkflow.Application.Interfaces;

namespace VMWorkflow.API.Controllers;

[Route("api/requests/{id:guid}/noc")]
[Authorize(Roles = "NOC,PlatformAdmin")]
public class NOCController : ApiControllerBase
{
    private readonly IRequestService _requestService;

    public NOCController(IRequestService requestService)
    {
        _requestService = requestService;
    }

    [HttpPost]
    public async Task<ActionResult<RequestResponseDto>> SubmitNOC(Guid id, [FromBody] NOCDetailsDto dto, [FromQuery] string? action = null)
    {
        var user = RequireAuthenticatedUsername();
        var result = action?.ToLower() == "save"
            ? await _requestService.SaveNOCAsync(id, dto, user)
            : await _requestService.SubmitNOCAsync(id, dto, user);
        return Ok(result);
    }
}
