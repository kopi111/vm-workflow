namespace VMWorkflow.Domain.Entities;

public class Vdom
{
    public Guid VdomId { get; set; }
    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
}
