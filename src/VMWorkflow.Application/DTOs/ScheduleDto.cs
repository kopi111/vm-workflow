using System.ComponentModel.DataAnnotations;
using VMWorkflow.Domain.Enums;

namespace VMWorkflow.Application.DTOs;

public class ScheduleDto
{
    public Guid? ScheduleId { get; set; }

    [Required, StringLength(50, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    public ScheduleType Type { get; set; }

    [StringLength(9)]
    public string? Color { get; set; }

    public DateTime? StartAt { get; set; }
    public DateTime? EndAt { get; set; }

    [StringLength(32)]
    public string? RecurrenceDays { get; set; }

    public bool PreExpirationEventLog { get; set; }

    [Range(0, 365)]
    public int? NumberOfDaysBefore { get; set; }
}
