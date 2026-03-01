using VMWorkflow.Domain.Enums;

namespace VMWorkflow.Domain.Entities;

public class StatusHistory
{
    public Guid StatusHistoryId { get; set; }
    public Guid RequestId { get; set; }

    public RequestStatus OldStatus { get; set; }
    public RequestStatus NewStatus { get; set; }
    public string ChangedBy { get; set; } = string.Empty;
    public string? Comments { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    // Navigation
    public Request Request { get; set; } = null!;
}
