using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using VMWorkflow.Application.DTOs;
using VMWorkflow.Application.Interfaces;
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
    private readonly ILdapAuthService _ldapAuth;
    private readonly ILogger<AuthController> _logger;

    public AuthController(WorkflowDbContext db, IConfiguration config, ILdapAuthService ldapAuth, ILogger<AuthController> logger)
    {
        _db = db;
        _config = config;
        _ldapAuth = ldapAuth;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto)
    {
        var adEnabled = _config.GetValue<bool>("ActiveDirectory:Enabled");

        if (adEnabled)
        {
            return await LoginWithAd(dto);
        }

        return await LoginWithLocal(dto);
    }

    private async Task<ActionResult<AuthResponseDto>> LoginWithAd(LoginDto dto)
    {
        var ldapUser = _ldapAuth.Authenticate(dto.Username, dto.Password);
        if (ldapUser == null)
            return Unauthorized(new { error = "Invalid username or password." });

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);

        if (user == null)
        {
            var role = ResolveRole(ldapUser.Groups);
            user = new User
            {
                UserId = Guid.NewGuid(),
                Username = ldapUser.Username,
                DisplayName = ldapUser.DisplayName,
                Email = ldapUser.Email,
                Role = role,
                PasswordHash = "AD_AUTH",
                IsBlocked = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Auto-provisioned AD user {Username} with role {Role}", user.Username, role);
        }
        else
        {
            user.DisplayName = ldapUser.DisplayName;
            user.Email = ldapUser.Email;
            user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        return BlockedAccountResponse(user) ?? Ok(GenerateToken(user));
    }

    private async Task<ActionResult<AuthResponseDto>> LoginWithLocal(LoginDto dto)
    {
        var user = await _db.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == dto.Username);

        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Unauthorized(new { error = "Invalid username or password." });

        return BlockedAccountResponse(user) ?? Ok(GenerateToken(user));
    }

    private ActionResult<AuthResponseDto>? BlockedAccountResponse(User user)
    {
        if (!user.IsBlocked)
            return null;
        return StatusCode(403, new { error = "Your account has been blocked. Contact an administrator." });
    }

    private string ResolveRole(List<string> adGroups)
    {
        var roleMapping = _config.GetSection("ActiveDirectory:RoleMapping")
            .GetChildren()
            .ToDictionary(x => x.Key, x => x.Value ?? "Requester");

        foreach (var (adGroup, appRole) in roleMapping)
        {
            if (adGroups.Any(g => g.Equals(adGroup, StringComparison.OrdinalIgnoreCase)))
                return appRole;
        }

        return _config["ActiveDirectory:DefaultRole"] ?? "Requester";
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
