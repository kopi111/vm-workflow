namespace VMWorkflow.Domain.Entities;

public class Script
{
    public Guid ScriptId { get; set; }
    public Guid RequestId { get; set; }
    public string ScriptType { get; set; } = "FortiGate";
    public string Content { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string GeneratedBy { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    public Request Request { get; set; } = null!;
}
