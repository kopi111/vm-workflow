using Microsoft.AspNetCore.Mvc;
using VMWorkflow.Application.DTOs;
using VMWorkflow.Application.Interfaces;

namespace VMWorkflow.API.Controllers;

[ApiController]
[Route("api/requests/{id:guid}/noc")]
public class NOCController : ControllerBase
{
    private readonly IRequestService _requestService;

    public NOCController(IRequestService requestService)
    {
        _requestService = requestService;
    }

    [HttpPost]
    public async Task<ActionResult<RequestResponseDto>> SubmitNOC(Guid id, [FromBody] NOCDetailsDto dto)
    {
        var user = User.Identity?.Name ?? "dev-user";
        var result = await _requestService.SubmitNOCAsync(id, dto, user);
        return Ok(result);
    }
}
