using System.ComponentModel.DataAnnotations;

namespace VMWorkflow.Application.DTOs;

public class DataCenterDetailsDto
{
    [Required, StringLength(100)]
    public string Environment { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string UplinkSpeed { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string BareMetalType { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string PortNumber { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string DC { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string RackRoom { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string RackNumber { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Cluster { get; set; } = string.Empty;
}
