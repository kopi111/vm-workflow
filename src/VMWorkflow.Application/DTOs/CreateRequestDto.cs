using VMWorkflow.Domain.Enums;

namespace VMWorkflow.Application.DTOs;

public class CreateRequestDto
{
    public string ApplicationName { get; set; } = string.Empty;
    public EnvironmentType Environment { get; set; }
    public string? ProgrammingLanguage { get; set; }
    public string? Framework { get; set; }
    public string? Purpose { get; set; }
    public int? ExpectedUsers { get; set; }
    public string? DBMS { get; set; }
    public string? GitRepoLink { get; set; }
    public string? AccessGroup { get; set; }
    public SLALevel SLA { get; set; } = SLALevel.Standard;
    public string? FQDNSuggestion { get; set; }
    public string? AuthenticationMethod { get; set; }
}
