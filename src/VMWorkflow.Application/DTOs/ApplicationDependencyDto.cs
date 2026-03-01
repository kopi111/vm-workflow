namespace VMWorkflow.Application.DTOs;

public class ApplicationDependencyDto
{
    public string DependencyName { get; set; } = string.Empty;
    public int Port { get; set; }
    public string Protocol { get; set; } = "TCP";
}
