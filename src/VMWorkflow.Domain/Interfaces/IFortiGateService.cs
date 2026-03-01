using VMWorkflow.Domain.Entities;

namespace VMWorkflow.Domain.Interfaces;

public interface IFortiGateService
{
    Task<string?> CreateAddressObjectAsync(Request request, NOCDetails nocDetails);
    Task<string?> CreateFirewallPolicyAsync(Request request, SOCDetails socDetails, NOCDetails nocDetails);
}
