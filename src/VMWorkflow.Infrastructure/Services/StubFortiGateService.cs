using Microsoft.Extensions.Logging;
using VMWorkflow.Domain.Entities;
using VMWorkflow.Domain.Interfaces;

namespace VMWorkflow.Infrastructure.Services;

public class StubFortiGateService : IFortiGateService
{
    private readonly ILogger<StubFortiGateService> _logger;

    public StubFortiGateService(ILogger<StubFortiGateService> logger)
    {
        _logger = logger;
    }

    public Task<string?> CreateAddressObjectAsync(Request request, NOCDetails nocDetails)
    {
        _logger.LogInformation("[STUB] FortiGate CreateAddressObject for {Slug} IP {IP} - simulated",
            request.ObjectSlug, nocDetails.IPAddress);
        var fakeId = $"fg-addr-{Guid.NewGuid():N}"[..16];
        return Task.FromResult<string?>(fakeId);
    }

    public Task<string?> CreateFirewallPolicyAsync(Request request, SOCDetails socDetails, NOCDetails nocDetails)
    {
        var firewallCount = socDetails.FirewallEntries.Count;
        _logger.LogInformation("[STUB] FortiGate CreateFirewallPolicy for {Slug} with {Count} firewall entries - simulated",
            request.ObjectSlug, firewallCount);
        var fakeId = $"fg-pol-{Guid.NewGuid():N}"[..16];
        return Task.FromResult<string?>(fakeId);
    }
}
