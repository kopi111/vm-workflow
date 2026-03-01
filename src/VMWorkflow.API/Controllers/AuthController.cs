using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using VMWorkflow.Application.DTOs;
using VMWorkflow.Domain.Entities;
using VMWorkflow.Infrastructure.Data;

namespace VMWorkflow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly WorkflowDbContext _db;
    private readonly IConfiguration _config;

    public AuthController(WorkflowDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto)
    {
        var user = await _db.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == dto.Username);

        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Unauthorized(new { error = "Invalid username or password." });

        if (user.IsBlocked)
            return StatusCode(403, new { error = "Your account has been blocked. Contact an administrator." });

        return Ok(GenerateToken(user));
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto dto)
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
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            IsBlocked = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return Ok(GenerateToken(user));
    }

    private AuthResponseDto GenerateToken(User user)
    {
        var jwtSection = _config.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(double.Parse(jwtSection["ExpireMinutes"]!));

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new Claim(JwtRegisteredClaimNames.Name, user.Username),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("displayName", user.DisplayName)
        };

        var token = new JwtSecurityToken(
            issuer: jwtSection["Issuer"],
            audience: jwtSection["Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: creds);

        return new AuthResponseDto
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Username = user.Username,
            DisplayName = user.DisplayName,
            Role = user.Role,
            ExpiresAt = expires
        };
    }
}
