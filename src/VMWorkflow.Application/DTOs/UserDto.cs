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
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? Password { get; set; }
}

public class ToggleBlockDto
{
    public bool IsBlocked { get; set; }
}

public class UpdateRoleDto
{
    public string Role { get; set; } = string.Empty;
}
