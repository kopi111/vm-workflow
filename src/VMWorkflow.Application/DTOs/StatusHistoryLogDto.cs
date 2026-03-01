using VMWorkflow.Domain.Enums;

namespace VMWorkflow.Application.DTOs;

public class StatusHistoryLogDto
{
    public Guid StatusHistoryId { get; set; }
    public Guid RequestId { get; set; }
    public string RequestNumber { get; set; } = string.Empty;
    public RequestStatus OldStatus { get; set; }
    public RequestStatus NewStatus { get; set; }
    public string ChangedBy { get; set; } = string.Empty;
    public string? Comments { get; set; }
    public DateTime Timestamp { get; set; }
}
