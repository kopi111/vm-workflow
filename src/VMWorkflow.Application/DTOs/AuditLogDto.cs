namespace VMWorkflow.Application.DTOs;

public class AuditLogDto
{
    public Guid AuditLogId { get; set; }
    public string User { get; set; } = string.Empty;
    public string HttpMethod { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public DateTime Timestamp { get; set; }
}
