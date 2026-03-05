using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VMWorkflow.Application.DTOs;
using VMWorkflow.Application.Interfaces;

namespace VMWorkflow.API.Controllers;

[ApiController]
[Route("api/queue")]
[Authorize]
public class QueueController : ControllerBase
{
    private readonly IRequestService _requestService;

    public QueueController(IRequestService requestService)
    {
        _requestService = requestService;
    }

    /// <summary>
    /// Get pending requests for a specific role/group.
    /// Valid roles: sysadmin, datacenter, noc, soc, ioc, ciso, ops
    /// </summary>
    [HttpGet("{role}")]
    public async Task<ActionResult<List<RequestResponseDto>>> GetPendingByRole(string role)
    {
        var results = await _requestService.GetPendingByRoleAsync(role);
        return Ok(results);
    }

    /// <summary>
    /// Get requests that were sent back by the current user.
    /// </summary>
    [HttpGet("sent-back")]
    public async Task<ActionResult<List<RequestResponseDto>>> GetSentBack()
    {
        var username = User.Identity?.Name ?? throw new UnauthorizedAccessException("User identity not available.");
        var results = await _requestService.GetSentBackByUserAsync(username);
        return Ok(results);
    }

    /// <summary>
    /// Get requests that were rejected by the current user.
    /// </summary>
    [HttpGet("rejected")]
    public async Task<ActionResult<List<RequestResponseDto>>> GetRejected()
    {
        var username = User.Identity?.Name ?? throw new UnauthorizedAccessException("User identity not available.");
        var results = await _requestService.GetRejectedByUserAsync(username);
        return Ok(results);
    }
}
