using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VMWorkflow.Application.DTOs;
using VMWorkflow.Application.Interfaces;

namespace VMWorkflow.API.Controllers;

[Route("api/requests/{id:guid}/ioc")]
[Authorize(Roles = "IOCManager,PlatformAdmin")]
public class IOCManagerController : ApiControllerBase
{
    private readonly IRequestService _requestService;

    public IOCManagerController(IRequestService requestService)
    {
        _requestService = requestService;
    }

    [HttpPost("submit")]
    public async Task<ActionResult<RequestResponseDto>> SubmitForApproval(Guid id, [FromBody] IOCSubmitDto dto)
    {
        var user = RequireAuthenticatedUsername();
        var result = await _requestService.ProcessIOCApprovalAsync(id, dto, user);
        return Ok(result);
    }

    [HttpPost("send-back")]
    public async Task<ActionResult<RequestResponseDto>> SendBack(Guid id, [FromBody] SendBackDto dto)
    {
        var user = RequireAuthenticatedUsername();
        var result = await _requestService.SendBackAsync(id, dto, user);
        return Ok(result);
    }

    [HttpPost("reject")]
    public async Task<ActionResult<RequestResponseDto>> Reject(Guid id, [FromBody] SendBackDto dto)
    {
        var user = RequireAuthenticatedUsername();
        var result = await _requestService.RejectAsync(id, dto, user);
        return Ok(result);
    }

    [HttpPost("unreject")]
    public async Task<ActionResult<RequestResponseDto>> Unreject(Guid id)
    {
        var user = RequireAuthenticatedUsername();
        var result = await _requestService.UnrejectAsync(id, user);
        return Ok(result);
    }
}
