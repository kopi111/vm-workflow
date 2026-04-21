using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VMWorkflow.Application.DTOs;
using VMWorkflow.Domain.Entities;
using VMWorkflow.Infrastructure.Data;

namespace VMWorkflow.API.Controllers;

[Route("api/admin")]
[Authorize(Roles = "SysAdmin,IOCManager,PlatformAdmin")]
public class AdminController : ApiControllerBase
{
    private readonly WorkflowDbContext _db;

    public AdminController(WorkflowDbContext db)
    {
        _db = db;
    }

[HttpGet("resource-groups")]
    [AllowAnonymous]
    public async Task<ActionResult<List<ResourceGroupDto>>> GetResourceGroups()
    {
        var groups = await _db.ResourceGroups.OrderBy(r => r.Name).ToListAsync();
        return Ok(groups.Select(r => new ResourceGroupDto
        {
            ResourceGroupId = r.ResourceGroupId,
            Name = r.Name,
            VCpu = r.VCpu,
            Ram = r.Ram,
            Hdd = r.Hdd
        }));
    }

    [HttpPost("resource-groups")]
    public async Task<ActionResult<ResourceGroupDto>> CreateResourceGroup([FromBody] ResourceGroupDto dto)
    {
        var entity = new ResourceGroup
        {
            ResourceGroupId = Guid.NewGuid(),
            Name = dto.Name,
            VCpu = dto.VCpu,
            Ram = dto.Ram,
            Hdd = dto.Hdd,
            CreatedBy = RequireAuthenticatedUsername(),
            CreatedAt = DateTime.UtcNow
        };
        _db.ResourceGroups.Add(entity);
        await _db.SaveChangesAsync();
        dto.ResourceGroupId = entity.ResourceGroupId;
        return CreatedAtAction(nameof(GetResourceGroups), dto);
    }

    [HttpPut("resource-groups/{id:guid}")]
    public async Task<ActionResult> UpdateResourceGroup(Guid id, [FromBody] ResourceGroupDto dto)
    {
        var entity = await _db.ResourceGroups.FindAsync(id);
        if (entity == null) return NotFound();
        entity.Name = dto.Name;
        entity.VCpu = dto.VCpu;
        entity.Ram = dto.Ram;
        entity.Hdd = dto.Hdd;
        await _db.SaveChangesAsync();
        return Ok(dto);
    }

    [HttpDelete("resource-groups/{id:guid}")]
    public async Task<ActionResult> DeleteResourceGroup(Guid id)
    {
        var entity = await _db.ResourceGroups.FindAsync(id);
        if (entity == null) return NotFound();
        _db.ResourceGroups.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("security-profiles")]
    [AllowAnonymous]
    public async Task<ActionResult<List<SecurityProfileDto>>> GetSecurityProfiles()
    {
        var profiles = await _db.SecurityProfiles.OrderBy(s => s.Name).ToListAsync();
        return Ok(profiles.Select(s => new SecurityProfileDto
        {
            SecurityProfileId = s.SecurityProfileId,
            Name = s.Name
        }));
    }

    [HttpPost("security-profiles")]
    public async Task<ActionResult<SecurityProfileDto>> CreateSecurityProfile([FromBody] SecurityProfileDto dto)
    {
        var entity = new SecurityProfile
        {
            SecurityProfileId = Guid.NewGuid(),
            Name = dto.Name,
            CreatedBy = RequireAuthenticatedUsername(),
            CreatedAt = DateTime.UtcNow
        };
        _db.SecurityProfiles.Add(entity);
        await _db.SaveChangesAsync();
        dto.SecurityProfileId = entity.SecurityProfileId;
        return CreatedAtAction(nameof(GetSecurityProfiles), dto);
    }

    [HttpPut("security-profiles/{id:guid}")]
    public async Task<ActionResult> UpdateSecurityProfile(Guid id, [FromBody] SecurityProfileDto dto)
    {
        var entity = await _db.SecurityProfiles.FindAsync(id);
        if (entity == null) return NotFound();
        entity.Name = dto.Name;
        await _db.SaveChangesAsync();
        return Ok(dto);
    }

    [HttpDelete("security-profiles/{id:guid}")]
    public async Task<ActionResult> DeleteSecurityProfile(Guid id)
    {
        var entity = await _db.SecurityProfiles.FindAsync(id);
        if (entity == null) return NotFound();
        _db.SecurityProfiles.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("vdoms")]
    [AllowAnonymous]
    public async Task<ActionResult<List<VdomDto>>> GetVdoms()
    {
        var vdoms = await _db.Vdoms.OrderBy(v => v.Name).ToListAsync();
        return Ok(vdoms.Select(v => new VdomDto
        {
            VdomId = v.VdomId,
            Name = v.Name
        }));
    }

    [HttpPost("vdoms")]
    public async Task<ActionResult<VdomDto>> CreateVdom([FromBody] VdomDto dto)
    {
        var entity = new Vdom
        {
            VdomId = Guid.NewGuid(),
            Name = dto.Name,
            CreatedBy = RequireAuthenticatedUsername(),
            CreatedAt = DateTime.UtcNow
        };
        _db.Vdoms.Add(entity);
        await _db.SaveChangesAsync();
        dto.VdomId = entity.VdomId;
        return CreatedAtAction(nameof(GetVdoms), dto);
    }

    [HttpPut("vdoms/{id:guid}")]
    public async Task<ActionResult> UpdateVdom(Guid id, [FromBody] VdomDto dto)
    {
        var entity = await _db.Vdoms.FindAsync(id);
        if (entity == null) return NotFound();
        entity.Name = dto.Name;
        await _db.SaveChangesAsync();
        return Ok(dto);
    }

    [HttpDelete("vdoms/{id:guid}")]
    public async Task<ActionResult> DeleteVdom(Guid id)
    {
        var entity = await _db.Vdoms.FindAsync(id);
        if (entity == null) return NotFound();
        _db.Vdoms.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("schedules")]
    [AllowAnonymous]
    public async Task<ActionResult<List<ScheduleDto>>> GetSchedules()
    {
        var schedules = await _db.Schedules
            .OrderBy(s => s.Type == VMWorkflow.Domain.Enums.ScheduleType.Always ? 0 : 1)
            .ThenBy(s => s.Name)
            .ToListAsync();

        return Ok(schedules.Select(MapSchedule));
    }

    [HttpPost("schedules")]
    public async Task<ActionResult<ScheduleDto>> CreateSchedule([FromBody] ScheduleDto dto)
    {
        if (string.Equals(dto.Name, "always", StringComparison.OrdinalIgnoreCase))
            return Conflict("The reserved name 'always' cannot be used.");

        if (await _db.Schedules.AnyAsync(s => s.Name == dto.Name))
            return Conflict($"A schedule named '{dto.Name}' already exists.");

        var entity = new Schedule
        {
            ScheduleId = Guid.NewGuid(),
            Name = dto.Name,
            Type = dto.Type,
            Color = dto.Color,
            StartAt = dto.StartAt,
            EndAt = dto.EndAt,
            RecurrenceDays = dto.RecurrenceDays,
            PreExpirationEventLog = dto.PreExpirationEventLog,
            NumberOfDaysBefore = dto.NumberOfDaysBefore,
            CreatedBy = RequireAuthenticatedUsername(),
            CreatedAt = DateTime.UtcNow
        };
        _db.Schedules.Add(entity);
        await _db.SaveChangesAsync();
        dto.ScheduleId = entity.ScheduleId;
        return CreatedAtAction(nameof(GetSchedules), dto);
    }

    [HttpPut("schedules/{id:guid}")]
    public async Task<ActionResult> UpdateSchedule(Guid id, [FromBody] ScheduleDto dto)
    {
        var entity = await _db.Schedules.FindAsync(id);
        if (entity == null) return NotFound();
        if (entity.Type == VMWorkflow.Domain.Enums.ScheduleType.Always)
            return BadRequest("The default 'always' schedule cannot be modified.");

        entity.Name = dto.Name;
        entity.Type = dto.Type;
        entity.Color = dto.Color;
        entity.StartAt = dto.StartAt;
        entity.EndAt = dto.EndAt;
        entity.RecurrenceDays = dto.RecurrenceDays;
        entity.PreExpirationEventLog = dto.PreExpirationEventLog;
        entity.NumberOfDaysBefore = dto.NumberOfDaysBefore;
        await _db.SaveChangesAsync();
        return Ok(dto);
    }

    [HttpDelete("schedules/{id:guid}")]
    public async Task<ActionResult> DeleteSchedule(Guid id)
    {
        var entity = await _db.Schedules.FindAsync(id);
        if (entity == null) return NotFound();
        if (entity.Type == VMWorkflow.Domain.Enums.ScheduleType.Always)
            return BadRequest("The default 'always' schedule cannot be deleted.");

        _db.Schedules.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static ScheduleDto MapSchedule(Schedule s) => new()
    {
        ScheduleId = s.ScheduleId,
        Name = s.Name,
        Type = s.Type,
        Color = s.Color,
        StartAt = s.StartAt,
        EndAt = s.EndAt,
        RecurrenceDays = s.RecurrenceDays,
        PreExpirationEventLog = s.PreExpirationEventLog,
        NumberOfDaysBefore = s.NumberOfDaysBefore
    };

    [HttpGet("dropdown-options/{category}")]
    [AllowAnonymous]
    public async Task<ActionResult<List<DropdownOptionDto>>> GetDropdownOptions(string category)
    {
        var options = await _db.DropdownOptions
            .Where(d => d.Category == category)
            .OrderBy(d => d.SortOrder)
            .ToListAsync();
        return Ok(options.Select(d => new DropdownOptionDto
        {
            DropdownOptionId = d.DropdownOptionId,
            Category = d.Category,
            Value = d.Value,
            SortOrder = d.SortOrder
        }));
    }

    [HttpPost("dropdown-options")]
    public async Task<ActionResult<DropdownOptionDto>> CreateDropdownOption([FromBody] DropdownOptionDto dto)
    {
        var entity = new DropdownOption
        {
            DropdownOptionId = Guid.NewGuid(),
            Category = dto.Category,
            Value = dto.Value,
            SortOrder = dto.SortOrder,
            CreatedBy = RequireAuthenticatedUsername(),
            CreatedAt = DateTime.UtcNow
        };
        _db.DropdownOptions.Add(entity);
        await _db.SaveChangesAsync();
        dto.DropdownOptionId = entity.DropdownOptionId;
        return CreatedAtAction(nameof(GetDropdownOptions), new { category = dto.Category }, dto);
    }

    [HttpPut("dropdown-options/{id:guid}")]
    public async Task<ActionResult> UpdateDropdownOption(Guid id, [FromBody] DropdownOptionDto dto)
    {
        var entity = await _db.DropdownOptions.FindAsync(id);
        if (entity == null) return NotFound();
        entity.Value = dto.Value;
        entity.SortOrder = dto.SortOrder;
        await _db.SaveChangesAsync();
        return Ok(dto);
    }

    [HttpDelete("dropdown-options/{id:guid}")]
    public async Task<ActionResult> DeleteDropdownOption(Guid id)
    {
        var entity = await _db.DropdownOptions.FindAsync(id);
        if (entity == null) return NotFound();
        _db.DropdownOptions.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
