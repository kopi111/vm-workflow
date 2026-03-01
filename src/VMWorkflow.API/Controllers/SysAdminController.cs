using Microsoft.AspNetCore.Mvc;
using VMWorkflow.Application.DTOs;
using VMWorkflow.Application.Interfaces;

namespace VMWorkflow.API.Controllers;

[ApiController]
[Route("api/requests/{id:guid}/sysadmin")]
public class SysAdminController : ControllerBase
{
    private readonly IRequestService _requestService;

    public SysAdminController(IRequestService requestService)
    {
        _requestService = requestService;
    }

    [HttpPost]
    public async Task<ActionResult<RequestResponseDto>> SubmitSysAdmin(Guid id, [FromBody] SysAdminDetailsDto dto)
    {
        var user = User.Identity?.Name ?? "dev-user";
        var result = await _requestService.SubmitSysAdminAsync(id, dto, user);
        return Ok(result);
    }
}
