namespace VMWorkflow.Application.DTOs;

public class ResourceGroupDto
{
    public Guid? ResourceGroupId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int VCpu { get; set; }
    public string Ram { get; set; } = string.Empty;
    public string Hdd { get; set; } = string.Empty;
}
