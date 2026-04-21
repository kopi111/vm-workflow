using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VMWorkflow.Application.DTOs;
using VMWorkflow.Application.Interfaces;

namespace VMWorkflow.API.Controllers;

[Route("api/queue")]
[Authorize]
public class QueueController : ApiControllerBase
{
    private readonly IRequestService _requestService;

    public QueueController(IRequestService requestService)
    {
        _requestService = requestService;
    }

    // Valid roles: sysadmin, datacenter, noc, soc, ioc, ciso, ops
    [HttpGet("{role}")]
    public async Task<ActionResult<List<RequestResponseDto>>> GetPendingByRole(string role)
    {
        var results = await _requestService.GetPendingByRoleAsync(role);
        return Ok(results);
    }

    [HttpGet("sent-back")]
    public async Task<ActionResult<List<RequestResponseDto>>> GetSentBack()
    {
        var username = RequireAuthenticatedUsername();
        var results = await _requestService.GetSentBackByUserAsync(username);
        return Ok(results);
    }

    [HttpGet("sent-back-to-me")]
    public async Task<ActionResult<List<RequestResponseDto>>> GetSentBackToMe()
    {
        var role = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;
        if (string.IsNullOrEmpty(role)) return Ok(new List<RequestResponseDto>());
        var key = role switch
        {
            "IOCManager" => "ioc",
            _ => role.ToLower()
        };
        var results = await _requestService.GetSentBackToRoleAsync(key);
        return Ok(results);
    }

    [HttpGet("rejected")]
    public async Task<ActionResult<List<RequestResponseDto>>> GetRejected()
    {
        var username = RequireAuthenticatedUsername();
        var results = await _requestService.GetRejectedByUserAsync(username);
        return Ok(results);
    }
}
