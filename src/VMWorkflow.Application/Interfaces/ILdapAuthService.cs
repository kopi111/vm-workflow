namespace VMWorkflow.Application.Interfaces;

public class LdapUserInfo
{
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> Groups { get; set; } = new();
}

public interface ILdapAuthService
{
    LdapUserInfo? Authenticate(string username, string password);
}
