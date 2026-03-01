using Microsoft.AspNetCore.Mvc;
using VMWorkflow.Application.DTOs;
using VMWorkflow.Application.Interfaces;

namespace VMWorkflow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RequestsController : ControllerBase
{
    private readonly IRequestService _requestService;

    public RequestsController(IRequestService requestService)
    {
        _requestService = requestService;
    }

    [HttpPost]
    public async Task<ActionResult<RequestResponseDto>> Create([FromBody] CreateRequestDto dto)
    {
        var user = GetCurrentUser();
        var result = await _requestService.CreateAsync(dto, user);
        return CreatedAtAction(nameof(GetById), new { id = result.RequestId }, result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RequestResponseDto>> GetById(Guid id)
    {
        var result = await _requestService.GetByIdAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<List<RequestResponseDto>>> GetAll()
    {
        var results = await _requestService.GetAllAsync();
        return Ok(results);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<RequestResponseDto>> Update(Guid id, [FromBody] UpdateRequestDto dto)
    {
        var user = GetCurrentUser();
        var result = await _requestService.UpdateAsync(id, dto, user);
        return Ok(result);
    }

    [HttpPost("{id:guid}/submit")]
    public async Task<ActionResult<RequestResponseDto>> Submit(Guid id)
    {
        var user = GetCurrentUser();
        var result = await _requestService.SubmitAsync(id, user);
        return Ok(result);
    }

    private string GetCurrentUser()
    {
        return User.Identity?.Name ?? "dev-user";
    }
}
