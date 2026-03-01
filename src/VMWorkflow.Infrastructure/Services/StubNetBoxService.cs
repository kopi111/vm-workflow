using Microsoft.Extensions.Logging;
using VMWorkflow.Domain.Entities;
using VMWorkflow.Domain.Interfaces;

namespace VMWorkflow.Infrastructure.Services;

public class StubNetBoxService : INetBoxService
{
    private readonly ILogger<StubNetBoxService> _logger;

    public StubNetBoxService(ILogger<StubNetBoxService> logger)
    {
        _logger = logger;
    }

    public Task<string?> CreateDeviceAsync(Request request)
    {
        _logger.LogInformation("[STUB] NetBox CreateDevice for {Slug} - simulated", request.ObjectSlug);
        var fakeId = $"nb-{Guid.NewGuid():N}"[..12];
        return Task.FromResult<string?>(fakeId);
    }

    public Task<bool> AssignIpAsync(string netBoxDeviceId, string ipAddress, string interfaceName)
    {
        _logger.LogInformation("[STUB] NetBox AssignIP {IP} to device {DeviceId} on {Interface} - simulated",
            ipAddress, netBoxDeviceId, interfaceName);
        return Task.FromResult(true);
    }
}
