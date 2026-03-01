using VMWorkflow.Domain.Enums;

namespace VMWorkflow.Domain.Services;

public static class ObjectSlugGenerator
{
    public static string Generate(string applicationName, EnvironmentType environment, int sequenceNumber)
    {
        var appPart = applicationName
            .Trim()
            .ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("_", "-");

        // Remove any non-alphanumeric/dash characters
        appPart = new string(appPart.Where(c => char.IsLetterOrDigit(c) || c == '-').ToArray());

        // Collapse multiple dashes
        while (appPart.Contains("--"))
            appPart = appPart.Replace("--", "-");

        appPart = appPart.Trim('-');

        var envPart = environment switch
        {
            EnvironmentType.Production => "prod",
            EnvironmentType.Staging => "staging",
            EnvironmentType.Development => "dev",
            EnvironmentType.DisasterRecovery => "dr",
            _ => "unknown"
        };

        return $"{appPart}-{envPart}-{sequenceNumber:D2}";
    }
}
