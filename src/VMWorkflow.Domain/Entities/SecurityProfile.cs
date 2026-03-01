namespace VMWorkflow.Domain.Entities;

public class SecurityProfile
{
    public Guid SecurityProfileId { get; set; }
    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
}
