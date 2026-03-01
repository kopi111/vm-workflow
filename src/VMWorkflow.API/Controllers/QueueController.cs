using Microsoft.AspNetCore.Mvc;
using VMWorkflow.Application.DTOs;
using VMWorkflow.Application.Interfaces;

namespace VMWorkflow.API.Controllers;

[ApiController]
[Route("api/queue")]
public class QueueController : ControllerBase
{
    private readonly IRequestService _requestService;

    public QueueController(IRequestService requestService)
    {
        _requestService = requestService;
    }

    /// <summary>
    /// Get pending requests for a specific role/group.
    /// Valid roles: sysadmin, datacenter, noc, soc, ioc, ciso, cto, ops
    /// </summary>
    [HttpGet("{role}")]
    public async Task<ActionResult<List<RequestResponseDto>>> GetPendingByRole(string role)
    {
        var results = await _requestService.GetPendingByRoleAsync(role);
        return Ok(results);
    }
}
