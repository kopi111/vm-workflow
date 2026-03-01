namespace VMWorkflow.Domain.Entities;

public class AutomationLog
{
    public Guid AutomationLogId { get; set; }
    public Guid RequestId { get; set; }

    public string Action { get; set; } = string.Empty;
    public string? Payload { get; set; }
    public string? Response { get; set; }
    public bool Success { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    // Navigation
    public Request Request { get; set; } = null!;
}
