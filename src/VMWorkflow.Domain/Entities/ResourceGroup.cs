namespace VMWorkflow.Domain.Entities;

public class ResourceGroup
{
    public Guid ResourceGroupId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int VCpu { get; set; }
    public string Ram { get; set; } = string.Empty;
    public string Hdd { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
}
