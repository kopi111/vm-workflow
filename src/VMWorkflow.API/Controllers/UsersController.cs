using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VMWorkflow.Application.DTOs;
using VMWorkflow.Domain.Entities;
using VMWorkflow.Infrastructure.Data;

namespace VMWorkflow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SysAdmin,PlatformAdmin")]
public class UsersController : ControllerBase
{
    private readonly WorkflowDbContext _db;

    public UsersController(WorkflowDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<UserDto>>> GetAll()
    {
        var users = await _db.Users
            .AsNoTracking()
            .OrderBy(u => u.Username)
            .Select(u => new UserDto
            {
                UserId = u.UserId,
                Username = u.Username,
                DisplayName = u.DisplayName,
                Email = u.Email,
                Role = u.Role,
                IsBlocked = u.IsBlocked,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt
            })
            .ToListAsync();

        return Ok(users);
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserDto dto)
    {
        var exists = await _db.Users.AnyAsync(u => u.Username == dto.Username);
        if (exists)
            return Conflict(new { error = $"Username '{dto.Username}' already exists." });

        var user = new User
        {
            UserId = Guid.NewGuid(),
            Username = dto.Username,
            DisplayName = dto.DisplayName,
            Email = dto.Email,
            Role = dto.Role,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password ?? "ChangeMe123!"),
            IsBlocked = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAll), new UserDto
        {
            UserId = user.UserId,
            Username = user.Username,
            DisplayName = user.DisplayName,
            Email = user.Email,
            Role = user.Role,
            IsBlocked = user.IsBlocked,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        });
    }

    [HttpPut("{id:guid}/role")]
    public async Task<ActionResult<UserDto>> UpdateRole(Guid id, [FromBody] UpdateRoleDto dto)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null)
            return NotFound(new { error = "User not found." });

        user.Role = dto.Role;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(new UserDto
        {
            UserId = user.UserId,
            Username = user.Username,
            DisplayName = user.DisplayName,
            Email = user.Email,
            Role = user.Role,
            IsBlocked = user.IsBlocked,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        });
    }

    [HttpPut("{id:guid}/block")]
    public async Task<ActionResult<UserDto>> ToggleBlock(Guid id, [FromBody] ToggleBlockDto dto)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null)
            return NotFound(new { error = "User not found." });

        user.IsBlocked = dto.IsBlocked;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(new UserDto
        {
            UserId = user.UserId,
            Username = user.Username,
            DisplayName = user.DisplayName,
            Email = user.Email,
            Role = user.Role,
            IsBlocked = user.IsBlocked,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        });
    }
}
