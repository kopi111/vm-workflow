using VMWorkflow.Domain.Enums;

namespace VMWorkflow.Domain.Entities;

public class Schedule
{
    public Guid ScheduleId { get; set; }
    public string Name { get; set; } = string.Empty;
    public ScheduleType Type { get; set; }
    public string? Color { get; set; }

    // One-Time / Recurring
    public DateTime? StartAt { get; set; }
    public DateTime? EndAt { get; set; }

    // Recurring
    public string? RecurrenceDays { get; set; }

    // Pre-expiration notification
    public bool PreExpirationEventLog { get; set; }
    public int? NumberOfDaysBefore { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
}
