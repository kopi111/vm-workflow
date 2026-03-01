namespace VMWorkflow.Domain.Entities;

public class ServiceEntry
{
    public Guid ServiceEntryId { get; set; }
    public Guid SysAdminDetailsId { get; set; }

    public string ServiceName { get; set; } = string.Empty; // e.g. HTTPS, SSH, DNS, UDP, FTP
    public string Port { get; set; } = string.Empty;        // e.g. 443, 22, 53
    public string Protocol { get; set; } = string.Empty;    // TCP, UDP, TCP/UDP

    // Navigation
    public SysAdminDetails SysAdminDetails { get; set; } = null!;
}
