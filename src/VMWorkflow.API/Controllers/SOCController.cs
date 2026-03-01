using Microsoft.AspNetCore.Mvc;
using VMWorkflow.Application.DTOs;
using VMWorkflow.Application.Interfaces;

namespace VMWorkflow.API.Controllers;

[ApiController]
[Route("api/requests/{id:guid}/soc")]
public class SOCController : ControllerBase
{
    private readonly IRequestService _requestService;

    public SOCController(IRequestService requestService)
    {
        _requestService = requestService;
    }

    [HttpPost]
    public async Task<ActionResult<RequestResponseDto>> SubmitSOC(Guid id, [FromBody] SOCDetailsDto dto)
    {
        var user = User.Identity?.Name ?? "dev-user";
        var result = await _requestService.SubmitSOCAsync(id, dto, user);
        return Ok(result);
    }
}
