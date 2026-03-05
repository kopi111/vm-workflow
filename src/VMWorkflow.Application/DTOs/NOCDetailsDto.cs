using System.ComponentModel.DataAnnotations;

namespace VMWorkflow.Application.DTOs;

public class NOCDetailsDto
{
    [Required(ErrorMessage = "Real IP Address is required."), StringLength(50)]
    public string IPAddress { get; set; } = string.Empty;

    [Required(ErrorMessage = "Real Subnet Mask is required."), StringLength(50)]
    public string SubnetMask { get; set; } = string.Empty;

    [Required(ErrorMessage = "Real VLAN ID is required."), StringLength(50)]
    public string VLANID { get; set; } = string.Empty;

    [Required(ErrorMessage = "Real Gateway is required."), StringLength(50)]
    public string Gateway { get; set; } = string.Empty;

    [Required(ErrorMessage = "Uplink Port is required."), StringLength(50)]
    public string Port { get; set; } = string.Empty;

    [StringLength(50)]
    public string VIP { get; set; } = string.Empty;

    [Required(ErrorMessage = "FQDN is required."), StringLength(500)]
    public string FQDN { get; set; } = string.Empty;

    [StringLength(50)]
    public string VirtualIP { get; set; } = string.Empty;

    [StringLength(50)]
    public string VirtualPort { get; set; } = string.Empty;

    [StringLength(500)]
    public string VirtualFQDN { get; set; } = string.Empty;

    public List<NetworkPathEntryDto> NetworkPaths { get; set; } = new();
}
