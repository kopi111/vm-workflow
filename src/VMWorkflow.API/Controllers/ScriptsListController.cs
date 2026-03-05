using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VMWorkflow.Infrastructure.Data;

namespace VMWorkflow.API.Controllers;

[ApiController]
[Route("api/scripts")]
[Authorize(Roles = "NOC,SOC,IOCManager,PlatformAdmin")]
public class ScriptsListController : ControllerBase
{
    private readonly WorkflowDbContext _db;

    public ScriptsListController(WorkflowDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var scripts = await _db.Scripts
            .Include(s => s.Request)
            .OrderByDescending(s => s.GeneratedAt)
            .Select(s => new
            {
                s.ScriptId,
                s.RequestId,
                RequestSlug = s.Request.ObjectSlug,
                ApplicationName = s.Request.ApplicationName,
                s.ScriptType,
                s.FileName,
                s.GeneratedBy,
                s.GeneratedAt
            })
            .ToListAsync();

        return Ok(scripts);
    }

    [HttpGet("{id:guid}/download")]
    public async Task<IActionResult> Download(Guid id)
    {
        var script = await _db.Scripts.FindAsync(id);
        if (script == null) return NotFound();

        return File(
            System.Text.Encoding.UTF8.GetBytes(script.Content),
            "text/plain",
            script.FileName
        );
    }
}
