using System.ComponentModel.DataAnnotations;

namespace VMWorkflow.Application.DTOs;

public class UserDto
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsBlocked { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateUserDto
{
    [Required, StringLength(100, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;

    [Required, StringLength(200, MinimumLength = 2)]
    public string DisplayName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string Role { get; set; } = string.Empty;

    [StringLength(200, MinimumLength = 6)]
    public string? Password { get; set; }
}

public class ToggleBlockDto
{
    public bool IsBlocked { get; set; }
}

public class UpdateRoleDto
{
    [Required, StringLength(50)]
    public string Role { get; set; } = string.Empty;
}
