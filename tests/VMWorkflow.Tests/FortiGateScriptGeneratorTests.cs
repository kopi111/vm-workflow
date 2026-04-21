using VMWorkflow.Application.Services;
using VMWorkflow.Domain.Entities;
using VMWorkflow.Domain.Enums;
using Xunit;

namespace VMWorkflow.Tests;

public class FortiGateScriptGeneratorTests
{
    private readonly FortiGateScriptGenerator _generator = new();

    private Request CreateApprovedRequest()
    {
        return new Request
        {
            RequestId = Guid.NewGuid(),
            ApplicationName = "Payroll",
            ObjectSlug = "payroll-prod-01",
            Status = RequestStatus.Approved,
            FQDNSuggestion = "payroll.corp.local",
            NOCDetails = new NOCDetails
            {
                IPAddress = "10.0.1.50",
                SubnetMask = "255.255.255.0",
                VLANID = "100",
                Gateway = "10.0.1.1",
                Port = "443",
                VIP = "10.0.1.100"
            },
            SOCDetails = new SOCDetails
            {
                FirewallEntries = new List<FirewallEntry>
                {
                    new() { FirewallEntryId = Guid.NewGuid(), PolicyName = "Allow-HTTPS", VDOM = "root", SourceInterface = "port1", DestinationInterface = "port2", SourceIP = "10.0.0.0/24", DestinationIP = "10.0.1.50", Services = new List<FirewallServiceEntry> { new() { FirewallServiceEntryId = Guid.NewGuid(), Port = "443", Protocol = "TCP", ServiceName = "HTTPS" } }, Schedule = "always", Action = PolicyAction.Accept },
                    new() { FirewallEntryId = Guid.NewGuid(), PolicyName = "Allow-HTTP", VDOM = "dmz", SourceInterface = "port3", DestinationInterface = "port4", SourceIP = "10.0.0.0/24", DestinationIP = "10.0.1.50", Services = new List<FirewallServiceEntry> { new() { FirewallServiceEntryId = Guid.NewGuid(), Port = "80", Protocol = "TCP", ServiceName = "HTTP" } }, Schedule = "always", Action = PolicyAction.Accept }
                }
            }
        };
    }

    [Fact]
    public void GenerateScript_ApprovedRequest_ContainsAddressObject()
    {
        var request = CreateApprovedRequest();
        var script = _generator.GenerateFortiGateScript(request);

        Assert.Contains("config firewall address", script);
        Assert.Contains("payroll-prod-01", script);
        Assert.Contains("10.0.1.50/32", script);
    }

    [Fact]
    public void GenerateScript_ApprovedRequest_ContainsFirewallPolicy()
    {
        var request = CreateApprovedRequest();
        var script = _generator.GenerateFortiGateScript(request);

        Assert.Contains("config firewall policy", script);
        Assert.Contains("Allow-HTTPS", script);
    }

    [Fact]
    public void GenerateScript_ApprovedRequest_ContainsFirewallEntries()
    {
        var request = CreateApprovedRequest();
        var script = _generator.GenerateFortiGateScript(request);

        Assert.Contains("Allow-HTTPS", script);
        Assert.Contains("Allow-HTTP", script);
        Assert.Contains("VDOM: root", script);
        Assert.Contains("VDOM: dmz", script);
    }

    [Fact]
    public void GenerateScript_DraftRequest_Throws()
    {
        var request = CreateApprovedRequest();
        request.Status = RequestStatus.Draft;

        Assert.Throws<InvalidOperationException>(() =>
            _generator.GenerateFortiGateScript(request));
    }

    [Fact]
    public void GenerateScript_NoNOCDetails_Throws()
    {
        var request = CreateApprovedRequest();
        request.NOCDetails = null;

        Assert.Throws<InvalidOperationException>(() =>
            _generator.GenerateFortiGateScript(request));
    }

    [Fact]
    public void GenerateScript_ImplementedStatus_Works()
    {
        var request = CreateApprovedRequest();
        request.Status = RequestStatus.Implemented;
        var script = _generator.GenerateFortiGateScript(request);

        Assert.Contains("payroll-prod-01", script);
    }
}
