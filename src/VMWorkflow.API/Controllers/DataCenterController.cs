using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VMWorkflow.Application.DTOs;
using VMWorkflow.Application.Interfaces;

namespace VMWorkflow.API.Controllers;

[Route("api/requests/{id:guid}/datacenter")]
[Authorize(Roles = "DataCenter,PlatformAdmin")]
public class DataCenterController : ApiControllerBase
{
    private readonly IRequestService _requestService;

    public DataCenterController(IRequestService requestService)
    {
        _requestService = requestService;
    }

    [HttpPost]
    public async Task<ActionResult<RequestResponseDto>> SubmitDataCenter(Guid id, [FromBody] DataCenterDetailsDto dto, [FromQuery] string? action = null)
    {
        var user = RequireAuthenticatedUsername();

        if (action?.ToLower() == "save")
        {
            ModelState.Clear();
            var saveResult = await _requestService.SaveDataCenterAsync(id, dto, user);
            return Ok(saveResult);
        }

        var result = await _requestService.SubmitDataCenterAsync(id, dto, user);
        return Ok(result);
    }
}
