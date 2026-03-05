using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VMWorkflow.Application.DTOs;
using VMWorkflow.Application.Interfaces;

namespace VMWorkflow.API.Controllers;

[ApiController]
[Route("api/requests/{id:guid}/ioc")]
[Authorize(Roles = "IOCManager,PlatformAdmin")]
public class IOCManagerController : ControllerBase
{
    private readonly IRequestService _requestService;

    public IOCManagerController(IRequestService requestService)
    {
        _requestService = requestService;
    }

    /// <summary>
    /// IOC Manager reviews and submits NOC+SOC to approval with comments.
    /// Only IOC Manager can submit from PendingIOCApproval to PendingApproval.
    /// </summary>
    [HttpPost("submit")]
    public async Task<ActionResult<RequestResponseDto>> SubmitForApproval(Guid id, [FromBody] IOCSubmitDto dto)
    {
        var user = User.Identity?.Name ?? throw new UnauthorizedAccessException("User identity not available.");
        var result = await _requestService.ProcessIOCApprovalAsync(id, dto, user);
        return Ok(result);
    }

    /// <summary>
    /// IOC Manager can send back to NOC/SOC.
    /// </summary>
    [HttpPost("send-back")]
    public async Task<ActionResult<RequestResponseDto>> SendBack(Guid id, [FromBody] SendBackDto dto)
    {
        var user = User.Identity?.Name ?? throw new UnauthorizedAccessException("User identity not available.");
        var result = await _requestService.SendBackAsync(id, dto, user);
        return Ok(result);
    }

    /// <summary>
    /// IOC Manager can reject the request entirely.
    /// </summary>
    [HttpPost("reject")]
    public async Task<ActionResult<RequestResponseDto>> Reject(Guid id, [FromBody] SendBackDto dto)
    {
        var user = User.Identity?.Name ?? throw new UnauthorizedAccessException("User identity not available.");
        var result = await _requestService.RejectAsync(id, dto, user);
        return Ok(result);
    }

    /// <summary>
    /// IOC Manager can unreject and restore to PendingIOCApproval.
    /// </summary>
    [HttpPost("unreject")]
    public async Task<ActionResult<RequestResponseDto>> Unreject(Guid id)
    {
        var user = User.Identity?.Name ?? throw new UnauthorizedAccessException("User identity not available.");
        var result = await _requestService.UnrejectAsync(id, user);
        return Ok(result);
    }
}
