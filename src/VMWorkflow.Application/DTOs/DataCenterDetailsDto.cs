using System.ComponentModel.DataAnnotations;

namespace VMWorkflow.Application.DTOs;

public class DataCenterDetailsDto
{
    [Required(ErrorMessage = "Environment is required."), StringLength(100)]
    public string Environment { get; set; } = string.Empty;

    [Required(ErrorMessage = "Uplink Speed is required."), StringLength(50)]
    public string UplinkSpeed { get; set; } = string.Empty;

    [Required(ErrorMessage = "Bare Metal Type is required."), StringLength(50)]
    public string BareMetalType { get; set; } = string.Empty;

    [Required(ErrorMessage = "Port Number is required."), StringLength(50)]
    public string PortNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Data Center (DC) is required."), StringLength(100)]
    public string DC { get; set; } = string.Empty;

    [Required(ErrorMessage = "Rack Row is required."), StringLength(100)]
    public string RackRoom { get; set; } = string.Empty;

    [Required(ErrorMessage = "Rack Number is required."), StringLength(50)]
    public string RackNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Cluster is required."), StringLength(100)]
    public string Cluster { get; set; } = string.Empty;
}
