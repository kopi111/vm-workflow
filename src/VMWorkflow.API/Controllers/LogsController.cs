using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VMWorkflow.Application.DTOs;
using VMWorkflow.Infrastructure.Data;

namespace VMWorkflow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SysAdmin,PlatformAdmin")]
public class LogsController : ControllerBase
{
    private readonly WorkflowDbContext _db;

    public LogsController(WorkflowDbContext db)
    {
        _db = db;
    }

    [HttpGet("audit")]
    public async Task<ActionResult<List<AuditLogDto>>> GetAuditLogs(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string? user)
    {
        var query = _db.AuditLogs.AsNoTracking().AsQueryable();

        if (from.HasValue)
            query = query.Where(a => a.Timestamp >= from.Value);
        if (to.HasValue)
            query = query.Where(a => a.Timestamp <= to.Value);
        if (!string.IsNullOrEmpty(user))
            query = query.Where(a => a.User == user);

        var results = await query
            .OrderByDescending(a => a.Timestamp)
            .Take(500)
            .Select(a => new AuditLogDto
            {
                AuditLogId = a.AuditLogId,
                User = a.User,
                HttpMethod = a.HttpMethod,
                Path = a.Path,
                StatusCode = a.StatusCode,
                Timestamp = a.Timestamp
            })
            .ToListAsync();

        return Ok(results);
    }

    [HttpGet("status-history")]
    public async Task<ActionResult<List<StatusHistoryLogDto>>> GetStatusHistory(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string? user)
    {
        var query = _db.StatusHistories
            .AsNoTracking()
            .Include(s => s.Request)
            .AsQueryable();

        if (from.HasValue)
            query = query.Where(s => s.Timestamp >= from.Value);
        if (to.HasValue)
            query = query.Where(s => s.Timestamp <= to.Value);
        if (!string.IsNullOrEmpty(user))
            query = query.Where(s => s.ChangedBy == user);

        var results = await query
            .OrderByDescending(s => s.Timestamp)
            .Take(500)
            .Select(s => new StatusHistoryLogDto
            {
                StatusHistoryId = s.StatusHistoryId,
                RequestId = s.RequestId,
                RequestNumber = s.Request.ObjectSlug,
                OldStatus = s.OldStatus,
                NewStatus = s.NewStatus,
                ChangedBy = s.ChangedBy,
                Comments = s.Comments,
                Timestamp = s.Timestamp
            })
            .ToListAsync();

        return Ok(results);
    }
}
