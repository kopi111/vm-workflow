namespace VMWorkflow.Application.DTOs;

public class DataCenterDetailsDto
{
    public string Environment { get; set; } = string.Empty;
    public string UplinkSpeed { get; set; } = string.Empty;
    public string BareMetalType { get; set; } = string.Empty;
    public string PortNumber { get; set; } = string.Empty;
    public string DC { get; set; } = string.Empty;
    public string RackRoom { get; set; } = string.Empty;
    public string RackNumber { get; set; } = string.Empty;
    public string Cluster { get; set; } = string.Empty;
}
