using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VMWorkflow.Application.DTOs;
using VMWorkflow.Domain.Entities;
using VMWorkflow.Infrastructure.Data;

namespace VMWorkflow.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "SysAdmin,IOCManager,PlatformAdmin")]
public class AdminController : ControllerBase
{
    private readonly WorkflowDbContext _db;

    public AdminController(WorkflowDbContext db)
    {
        _db = db;
    }

    // ===== Resource Groups =====

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
            CreatedBy = User.Identity?.Name ?? throw new UnauthorizedAccessException("User identity not available."),
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

    // ===== Security Profiles =====

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
            CreatedBy = User.Identity?.Name ?? throw new UnauthorizedAccessException("User identity not available."),
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

    // ===== VDOMs =====

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
            CreatedBy = User.Identity?.Name ?? throw new UnauthorizedAccessException("User identity not available."),
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

    // ===== Dropdown Options =====

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
            CreatedBy = User.Identity?.Name ?? throw new UnauthorizedAccessException("User identity not available."),
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
