using Novell.Directory.Ldap;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VMWorkflow.Application.Interfaces;

namespace VMWorkflow.Infrastructure.Services;

public class LdapAuthService : ILdapAuthService
{
    private readonly IConfiguration _config;
    private readonly ILogger<LdapAuthService> _logger;

    public LdapAuthService(IConfiguration config, ILogger<LdapAuthService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public LdapUserInfo? Authenticate(string username, string password)
    {
        var ad = _config.GetSection("ActiveDirectory");
        var domain = ad["Domain"] ?? throw new InvalidOperationException("ActiveDirectory:Domain not configured.");
        var ldapServer = ad["LdapServer"] ?? throw new InvalidOperationException("ActiveDirectory:LdapServer not configured.");
        var ldapPort = int.Parse(ad["LdapPort"] ?? "389");
        var searchBase = ad["SearchBase"] ?? throw new InvalidOperationException("ActiveDirectory:SearchBase not configured.");
        var useSsl = bool.Parse(ad["UseSsl"] ?? "false");

        try
        {
            using var connection = new LdapConnection();

            if (useSsl)
            {
                connection.SecureSocketLayer = true;
            }

            connection.Connect(ldapServer, ldapPort);

            var userDn = $"{username}@{domain}";
            connection.Bind(userDn, password);

            var searchFilter = $"(&(objectClass=user)(sAMAccountName={EscapeLdapFilter(username)}))";
            var results = connection.Search(
                searchBase,
                LdapConnection.ScopeSub,
                searchFilter,
                new[] { "sAMAccountName", "displayName", "mail", "memberOf" },
                false);

            if (!results.HasMore())
            {
                _logger.LogWarning("LDAP bind succeeded but user {Username} not found in search base {SearchBase}", username, searchBase);
                return null;
            }

            var entry = results.Next();

            var displayName = GetAttribute(entry, "displayName") ?? username;
            var email = GetAttribute(entry, "mail") ?? $"{username}@{domain}";

            var groups = new List<string>();
            var memberOfAttr = entry.GetAttributeSet().ContainsKey("memberOf")
                ? entry.GetAttribute("memberOf")
                : null;

            if (memberOfAttr != null)
            {
                foreach (var memberOf in memberOfAttr.StringValueArray)
                {
                    var cn = memberOf.Split(',')
                        .FirstOrDefault(p => p.TrimStart().StartsWith("CN=", StringComparison.OrdinalIgnoreCase));
                    if (cn != null)
                        groups.Add(cn.Substring(cn.IndexOf('=') + 1));
                }
            }

            _logger.LogInformation("LDAP authentication successful for {Username}, groups: {Groups}",
                username, string.Join(", ", groups));

            return new LdapUserInfo
            {
                Username = username,
                DisplayName = displayName,
                Email = email,
                Groups = groups
            };
        }
        catch (LdapException ex)
        {
            _logger.LogWarning("LDAP authentication failed for {Username}: {Message}", username, ex.Message);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during LDAP authentication for {Username}", username);
            return null;
        }
    }

    private static string? GetAttribute(LdapEntry entry, string name)
    {
        if (entry.GetAttributeSet().ContainsKey(name))
        {
            return entry.GetAttribute(name).StringValue;
        }
        return null;
    }

    private static string EscapeLdapFilter(string input)
    {
        return input
            .Replace("\\", "\\5c")
            .Replace("*", "\\2a")
            .Replace("(", "\\28")
            .Replace(")", "\\29")
            .Replace("\0", "\\00");
    }
}
