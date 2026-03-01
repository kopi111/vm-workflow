using VMWorkflow.Domain.Entities;

namespace VMWorkflow.Domain.Interfaces;

public interface INetBoxService
{
    Task<string?> CreateDeviceAsync(Request request);
    Task<bool> AssignIpAsync(string netBoxDeviceId, string ipAddress, string interfaceName);
}
