using Microsoft.AspNetCore.Mvc;
using VMWorkflow.Application.DTOs;
using VMWorkflow.Application.Interfaces;

namespace VMWorkflow.API.Controllers;

[ApiController]
[Route("api/requests/{id:guid}/datacenter")]
public class DataCenterController : ControllerBase
{
    private readonly IRequestService _requestService;

    public DataCenterController(IRequestService requestService)
    {
        _requestService = requestService;
    }

    [HttpPost]
    public async Task<ActionResult<RequestResponseDto>> SubmitDataCenter(Guid id, [FromBody] DataCenterDetailsDto dto)
    {
        var user = User.Identity?.Name ?? "dev-user";
        var result = await _requestService.SubmitDataCenterAsync(id, dto, user);
        return Ok(result);
    }
}
