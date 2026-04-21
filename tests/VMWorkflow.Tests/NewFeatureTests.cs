using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using VMWorkflow.Application.DTOs;
using VMWorkflow.Application.Services;
using VMWorkflow.Domain.Enums;
using VMWorkflow.Infrastructure.Data;
using VMWorkflow.Infrastructure.Services;
using Xunit;

namespace VMWorkflow.Tests;

public class NewFeatureTests : IDisposable
{
    private readonly WorkflowDbContext _db;
    private readonly RequestService _service;

    public NewFeatureTests()
    {
        var options = new DbContextOptionsBuilder<WorkflowDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _db = new WorkflowDbContext(options);
        _service = new RequestService(_db, new WorkflowEngine(), new FortiGateScriptGenerator(), NullLogger<RequestService>.Instance);
    }

    public void Dispose() => _db.Dispose();

    private static CreateRequestDto FullDraft(string appName, EnvironmentType env = EnvironmentType.Production) => new()
    {
        ApplicationName = appName,
        Environment = env,
        ProgrammingLanguage = "C#",
        Framework = ".NET 8",
        Purpose = "Internal",
        ExpectedUsers = 25,
        DBMS = "PostgreSQL",
        GitRepoLink = "https://example.com/repo.git",
        AccessGroup = "dev-team",
        SLA = SLALevel.Standard,
        FQDNSuggestion = "app.corp.local",
        AuthenticationMethod = "LDAP"
    };

    // #1 - Rename application name regenerates ObjectSlug
    [Fact]
    public async Task UpdateAsync_RenameApplication_RegeneratesSlug()
    {
        var created = await _service.CreateAsync(FullDraft("OriginalName"), "alice");
        var originalSlug = created.ObjectSlug;

        var updated = await _service.UpdateAsync(created.RequestId,
            new UpdateRequestDto { ApplicationName = "RenamedApp" }, "alice");

        Assert.NotEqual(originalSlug, updated.ObjectSlug);
        Assert.StartsWith("renamedapp-", updated.ObjectSlug);
    }

    // #2 - Only creator may submit a draft
    [Fact]
    public async Task SubmitAsync_ByNonCreator_ThrowsUnauthorized()
    {
        var created = await _service.CreateAsync(FullDraft("MyApp"), "alice");

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.SubmitAsync(created.RequestId, "bob"));
    }

    [Fact]
    public async Task SubmitAsync_ByCreator_Succeeds()
    {
        var created = await _service.CreateAsync(FullDraft("MyApp"), "alice");

        var result = await _service.SubmitAsync(created.RequestId, "alice");

        Assert.Equal(RequestStatus.PendingSysAdmin, result.Status);
    }

    // #3 - Drafts scoped to creator
    [Fact]
    public async Task GetDraftsByUserAsync_OnlyReturnsOwnDrafts()
    {
        await _service.CreateAsync(FullDraft("AliceApp1"), "alice");
        await _service.CreateAsync(FullDraft("AliceApp2"), "alice");
        await _service.CreateAsync(FullDraft("BobApp"), "bob");

        var aliceDrafts = await _service.GetDraftsByUserAsync("alice");

        Assert.Equal(2, aliceDrafts.Count);
        Assert.All(aliceDrafts, d => Assert.Equal("alice", d.CreatedBy));
    }

    // #4 - Submit blocks when required fields are missing
    [Fact]
    public async Task SubmitAsync_MissingRequiredFields_Throws()
    {
        var dto = new CreateRequestDto { ApplicationName = "Incomplete", Environment = EnvironmentType.Production };
        var created = await _service.CreateAsync(dto, "alice");

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.SubmitAsync(created.RequestId, "alice"));

        Assert.Contains("Programming Language", ex.Message);
        Assert.Contains("Framework", ex.Message);
    }

    // #15 - IOC queue only returns PendingIOCApproval items
    [Fact]
    public async Task GetPendingByRoleAsync_Ioc_ReturnsOnlyIOCApproval()
    {
        var a = await _service.CreateAsync(FullDraft("A"), "alice");
        var b = await _service.CreateAsync(FullDraft("B"), "alice");
        var c = await _service.CreateAsync(FullDraft("C"), "alice");

        await _service.SubmitAsync(a.RequestId, "alice"); // PendingSysAdmin
        await _service.SubmitAsync(b.RequestId, "alice");
        await _service.SubmitAsync(c.RequestId, "alice");

        // Move 'c' all the way to PendingIOCApproval
        await _service.SubmitSysAdminAsync(c.RequestId,
            new SysAdminDetailsDto { SensitivityLevel = "Low", ServerResources = "Small", WebServer = "Apache", Hostname = "h" }, "sa");
        await _service.SubmitDataCenterAsync(c.RequestId,
            new DataCenterDetailsDto { Environment = "Dell", UplinkSpeed = "1Gbps", BareMetalType = "VM", PortNumber = "80", DC = "DC1", RackRoom = "R1", RackNumber = "R01", Cluster = "HF" }, "dc");
        await _service.SubmitNOCAsync(c.RequestId,
            new NOCDetailsDto { IPAddress = "10.0.0.5", SubnetMask = "255.255.255.0", VLANID = "10", Gateway = "10.0.0.254", Port = "80", VIP = "10.0.0.100" }, "noc");
        await _service.SubmitSOCAsync(c.RequestId,
            new SOCDetailsDto { FirewallEntries = new() { new() { PolicyName = "P1", VDOM = "root", Action = "Accept", Schedule = "always" } } }, "soc");

        var iocQueue = await _service.GetPendingByRoleAsync("ioc");

        Assert.Single(iocQueue);
        Assert.Equal(c.RequestId, iocQueue[0].RequestId);
        Assert.Equal(RequestStatus.PendingIOCApproval, iocQueue[0].Status);
    }

    [Fact]
    public async Task GetPendingByRoleAsync_NocSoc_StillIncludeBothPending()
    {
        var a = await _service.CreateAsync(FullDraft("A"), "alice");
        await _service.SubmitAsync(a.RequestId, "alice");
        await _service.SubmitSysAdminAsync(a.RequestId,
            new SysAdminDetailsDto { SensitivityLevel = "Low", ServerResources = "Small", WebServer = "Apache", Hostname = "h" }, "sa");
        await _service.SubmitDataCenterAsync(a.RequestId,
            new DataCenterDetailsDto { Environment = "Dell", UplinkSpeed = "1Gbps", BareMetalType = "VM", PortNumber = "80", DC = "DC1", RackRoom = "R1", RackNumber = "R01", Cluster = "HF" }, "dc");

        var nocQueue = await _service.GetPendingByRoleAsync("noc");
        var socQueue = await _service.GetPendingByRoleAsync("soc");

        Assert.Single(nocQueue);
        Assert.Single(socQueue);
    }
}
