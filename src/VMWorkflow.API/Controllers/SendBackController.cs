using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VMWorkflow.Application.DTOs;
using VMWorkflow.Application.Interfaces;

namespace VMWorkflow.API.Controllers;

[ApiController]
[Route("api/requests/{id:guid}/send-back")]
[Authorize]
public class SendBackController : ControllerBase
{
    private readonly IRequestService _requestService;

    public SendBackController(IRequestService requestService)
    {
        _requestService = requestService;
    }

    [HttpPost]
    public async Task<ActionResult<RequestResponseDto>> SendBack(Guid id, [FromBody] SendBackDto dto)
    {
        var user = User.Identity?.Name ?? throw new UnauthorizedAccessException("User identity not available.");
        var result = await _requestService.SendBackAsync(id, dto, user);
        return Ok(result);
    }
}
