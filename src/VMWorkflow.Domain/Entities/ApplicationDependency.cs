namespace VMWorkflow.Domain.Entities;

public class ApplicationDependency
{
    public Guid ApplicationDependencyId { get; set; }
    public Guid RequestId { get; set; }

    public string DependencyName { get; set; } = string.Empty;
    public int Port { get; set; }
    public string Protocol { get; set; } = "TCP";

    // Navigation
    public Request Request { get; set; } = null!;
}
