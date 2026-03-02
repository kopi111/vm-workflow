using System.ComponentModel.DataAnnotations;
using VMWorkflow.Domain.Enums;

namespace VMWorkflow.Application.DTOs;

public class UpdateRequestDto
{
    [StringLength(200, MinimumLength = 2)]
    public string? ApplicationName { get; set; }

    [StringLength(100)]
    public string? ProgrammingLanguage { get; set; }

    [StringLength(100)]
    public string? Framework { get; set; }

    [StringLength(2000)]
    public string? Purpose { get; set; }

    [Range(0, 1000000)]
    public int? ExpectedUsers { get; set; }

    [StringLength(100)]
    public string? DBMS { get; set; }

    [StringLength(500)]
    public string? GitRepoLink { get; set; }

    [StringLength(200)]
    public string? AccessGroup { get; set; }

    public SLALevel? SLA { get; set; }

    [StringLength(500)]
    public string? FQDNSuggestion { get; set; }

    [StringLength(100)]
    public string? AuthenticationMethod { get; set; }
}
