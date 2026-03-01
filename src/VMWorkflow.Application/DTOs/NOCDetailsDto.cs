namespace VMWorkflow.Application.DTOs;

public class NOCDetailsDto
{
    public string IPAddress { get; set; } = string.Empty;
    public string SubnetMask { get; set; } = string.Empty;
    public string VLANID { get; set; } = string.Empty;
    public string Gateway { get; set; } = string.Empty;
    public string Port { get; set; } = string.Empty;
    public string VIP { get; set; } = string.Empty;
    public string FQDN { get; set; } = string.Empty;
    public List<NetworkPathEntryDto> NetworkPaths { get; set; } = new();
}
