using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VMWorkflow.Application.Interfaces;
using VMWorkflow.Infrastructure.Data;

namespace VMWorkflow.API.Controllers;

[Route("api/requests/{id:guid}/generate-script")]
[Authorize(Roles = "NOC,SOC,IOCManager,SysAdmin,PlatformAdmin")]
public class ScriptController : ApiControllerBase
{
    private readonly WorkflowDbContext _db;
    private readonly IScriptGenerationService _scriptService;

    public ScriptController(WorkflowDbContext db, IScriptGenerationService scriptService)
    {
        _db = db;
        _scriptService = scriptService;
    }

    [HttpGet]
    public async Task<IActionResult> GenerateScript(Guid id)
    {
        var request = await _db.Requests
            .Include(r => r.NOCDetails)
            .Include(r => r.SOCDetails)
                .ThenInclude(s => s!.FirewallEntries)
                    .ThenInclude(f => f.Services)
            .FirstOrDefaultAsync(r => r.RequestId == id);

        if (request == null) return NotFound();

        var script = _scriptService.GenerateFortiGateScript(request);

        return File(
            System.Text.Encoding.UTF8.GetBytes(script),
            "text/plain",
            $"{request.ObjectSlug}-fortigate.conf"
        );
    }
}
