using VMWorkflow.Domain.Enums;

namespace VMWorkflow.Application.DTOs;

public class StatusHistoryDto
{
    public RequestStatus OldStatus { get; set; }
    public RequestStatus NewStatus { get; set; }
    public string ChangedBy { get; set; } = string.Empty;
    public string? Comments { get; set; }
    public DateTime Timestamp { get; set; }
}
