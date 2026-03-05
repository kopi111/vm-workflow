using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using VMWorkflow.Application.DTOs;
using VMWorkflow.Application.Services;
using VMWorkflow.Domain.Enums;
using VMWorkflow.Infrastructure.Data;
using VMWorkflow.Infrastructure.Services;
using Xunit;

namespace VMWorkflow.Tests;

public class RequestServiceTests : IDisposable
{
    private readonly WorkflowDbContext _db;
    private readonly RequestService _service;

    public RequestServiceTests()
    {
        var options = new DbContextOptionsBuilder<WorkflowDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _db = new WorkflowDbContext(options);
        var workflow = new WorkflowEngine();
        var scriptService = new FortiGateScriptGenerator();
        var logger = NullLogger<RequestService>.Instance;
        _service = new RequestService(_db, workflow, scriptService, logger);
    }

    public void Dispose()
    {
        _db.Dispose();
    }

    [Fact]
    public async Task CreateAsync_SetsStatusToDraft()
    {
        var dto = new CreateRequestDto
        {
            ApplicationName = "TestApp",
            Environment = EnvironmentType.Production
        };

        var result = await _service.CreateAsync(dto, "admin");

        Assert.Equal(RequestStatus.Draft, result.Status);
        Assert.Equal("testapp-prod-01", result.ObjectSlug);
        Assert.Equal("admin", result.CreatedBy);
    }

    [Fact]
    public async Task CreateAsync_WithNewFields_SavesThem()
    {
        var dto = new CreateRequestDto
        {
            ApplicationName = "MyApp",
            Environment = EnvironmentType.Development,
            ProgrammingLanguage = "Java",
            Framework = "Spring Boot",
            Purpose = "Payment processing",
            ExpectedUsers = 500,
            DBMS = "PostgreSQL",
            GitRepoLink = "https://git.corp.local/myapp",
            AccessGroup = "APP-MyApp-Users",
            SLA = SLALevel.Critical,
            FQDNSuggestion = "myapp.corp.local",
            AuthenticationMethod = "LDAP"
        };

        var result = await _service.CreateAsync(dto, "admin");

        Assert.Equal("Java", result.ProgrammingLanguage);
        Assert.Equal("Spring Boot", result.Framework);
        Assert.Equal("Payment processing", result.Purpose);
        Assert.Equal(500, result.ExpectedUsers);
        Assert.Equal("PostgreSQL", result.DBMS);
        Assert.Equal("https://git.corp.local/myapp", result.GitRepoLink);
        Assert.Equal("APP-MyApp-Users", result.AccessGroup);
        Assert.Equal(SLALevel.Critical, result.SLA);
        Assert.Equal("myapp.corp.local", result.FQDNSuggestion);
        Assert.Equal("LDAP", result.AuthenticationMethod);
    }

    [Fact]
    public async Task CreateAsync_DuplicateAppEnv_IncrementsSlug()
    {
        var dto = new CreateRequestDto { ApplicationName = "App", Environment = EnvironmentType.Production };

        var r1 = await _service.CreateAsync(dto, "admin");
        var r2 = await _service.CreateAsync(dto, "admin");

        Assert.Equal("app-prod-01", r1.ObjectSlug);
        Assert.Equal("app-prod-02", r2.ObjectSlug);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingRequest_ReturnsDto()
    {
        var dto = new CreateRequestDto { ApplicationName = "Test", Environment = EnvironmentType.Staging };
        var created = await _service.CreateAsync(dto, "admin");

        var result = await _service.GetByIdAsync(created.RequestId);

        Assert.NotNull(result);
        Assert.Equal(created.RequestId, result!.RequestId);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistent_ReturnsNull()
    {
        var result = await _service.GetByIdAsync(Guid.NewGuid());
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_DraftRequest_Succeeds()
    {
        var dto = new CreateRequestDto { ApplicationName = "App", Environment = EnvironmentType.Production };
        var created = await _service.CreateAsync(dto, "admin");

        var update = new UpdateRequestDto { ProgrammingLanguage = "C#", Framework = ".NET 8" };
        var result = await _service.UpdateAsync(created.RequestId, update, "admin");

        Assert.Equal("C#", result.ProgrammingLanguage);
        Assert.Equal(".NET 8", result.Framework);
    }

    [Fact]
    public async Task SubmitAsync_DraftRequest_TransitionsToPendingSysAdmin()
    {
        var dto = new CreateRequestDto { ApplicationName = "App", Environment = EnvironmentType.Production };
        var created = await _service.CreateAsync(dto, "admin");

        var result = await _service.SubmitAsync(created.RequestId, "admin");

        Assert.Equal(RequestStatus.PendingSysAdmin, result.Status);
    }

    [Fact]
    public async Task SubmitSysAdminAsync_SetsDataCenterReview()
    {
        var created = await _service.CreateAsync(
            new CreateRequestDto { ApplicationName = "TestApp", Environment = EnvironmentType.Production }, "admin");
        await _service.SubmitAsync(created.RequestId, "admin");

        var result = await _service.SubmitSysAdminAsync(created.RequestId,
            new SysAdminDetailsDto
            {
                SensitivityLevel = "High",
                ServerResources = "Medium",
                WebServer = "IIS",
                DatabaseName = "TestApp_Database",
                DatabaseUsername = "TestApp_User",
                Hostname = "srv-testapp-01"
            }, "sysadmin");

        Assert.Equal(RequestStatus.DataCenterReview, result.Status);
        Assert.NotNull(result.SysAdminDetails);
        Assert.Equal("High", result.SysAdminDetails!.SensitivityLevel);
        Assert.Equal("Medium", result.SysAdminDetails.ServerResources);
        Assert.Equal("IIS", result.SysAdminDetails.WebServer);
    }

    [Fact]
    public async Task FullWorkflow_DraftToApproved()
    {
        // Create
        var created = await _service.CreateAsync(
            new CreateRequestDto { ApplicationName = "Payroll", Environment = EnvironmentType.Production },
            "sysadmin");
        Assert.Equal(RequestStatus.Draft, created.Status);

        // Submit → PendingSysAdmin
        var submitted = await _service.SubmitAsync(created.RequestId, "sysadmin");
        Assert.Equal(RequestStatus.PendingSysAdmin, submitted.Status);

        // SysAdmin → DataCenterReview
        var sysAdmin = await _service.SubmitSysAdminAsync(created.RequestId,
            new SysAdminDetailsDto
            {
                SensitivityLevel = "Medium",
                ServerResources = "Large",
                WebServer = "Nginx",
                DatabaseName = "Payroll_Database",
                DatabaseUsername = "Payroll_User",
                Hostname = "srv-payroll-01"
            }, "sysadmin");
        Assert.Equal(RequestStatus.DataCenterReview, sysAdmin.Status);

        // DataCenter → PendingNOC
        var dc = await _service.SubmitDataCenterAsync(created.RequestId,
            new DataCenterDetailsDto
            {
                Environment = "Dell",
                UplinkSpeed = "10Gbps",
                BareMetalType = "VM",
                PortNumber = "443",
                DC = "DC1",
                RackRoom = "Room-A",
                RackNumber = "R01",
                Cluster = "HyperFlex"
            }, "dc-engineer");
        Assert.Equal(RequestStatus.PendingNOC, dc.Status);

        // NOC
        var noc = await _service.SubmitNOCAsync(created.RequestId,
            new NOCDetailsDto
            {
                IPAddress = "10.0.1.50",
                SubnetMask = "255.255.255.0",
                VLANID = "100",
                Gateway = "10.0.1.1",
                Port = "443",
                VIP = "10.0.1.100",
                FQDN = "payroll.corp.local",
                NetworkPaths = new List<NetworkPathEntryDto>
                {
                    new() { SwitchName = "SW-Core-01", Port = "Gi0/1", LinkSpeed = "10Gbps" }
                }
            }, "noc-engineer");
        Assert.Equal(RequestStatus.PendingSOC, noc.Status);

        // SOC
        var soc = await _service.SubmitSOCAsync(created.RequestId,
            new SOCDetailsDto
            {
                FirewallEntries = new List<FirewallEntryDto>
                {
                    new() { PolicyName = "Allow-HTTPS", VDOM = "root", Action = "Accept" }
                }
            }, "soc-engineer");
        Assert.Equal(RequestStatus.PendingIOCApproval, soc.Status);

        // IOC Approve → PendingApproval
        var iocApproved = await _service.ProcessIOCApprovalAsync(created.RequestId,
            new IOCSubmitDto { Comments = "IOC approved" },
            "ioc-manager");
        Assert.Equal(RequestStatus.PendingApproval, iocApproved.Status);

        // CISO Approve (1 of 2 — awaiting Ops Manager)
        var cisoApproved = await _service.ProcessApprovalAsync(created.RequestId,
            new ApprovalDto { Decision = ApprovalDecision.Approve, Comments = "CISO approved" },
            "ciso", "ciso");
        Assert.Equal(RequestStatus.PendingApproval, cisoApproved.Status);

        // Ops Manager Approve (2 of 2 — both approved → Approved)
        var opsApproved = await _service.ProcessApprovalAsync(created.RequestId,
            new ApprovalDto { Decision = ApprovalDecision.Approve, Comments = "Ops Manager approved" },
            "ops", "ops");
        Assert.Equal(RequestStatus.Approved, opsApproved.Status);
    }

    [Fact]
    public async Task BothApproval_CISOAndOps_SetsApproved()
    {
        // Setup: get to PendingApproval
        var created = await SetupToPendingApproval();

        // CISO approves (1 of 2)
        var r1 = await _service.ProcessApprovalAsync(created.RequestId,
            new ApprovalDto { Decision = ApprovalDecision.Approve, Comments = "CISO ok" },
            "ciso", "ciso");
        Assert.Equal(RequestStatus.PendingApproval, r1.Status);

        // Ops Manager approves (2 of 2 — both approved)
        var r2 = await _service.ProcessApprovalAsync(created.RequestId,
            new ApprovalDto { Decision = ApprovalDecision.Approve, Comments = "Ops ok" },
            "ops-officer", "ops");
        Assert.Equal(RequestStatus.Approved, r2.Status);
    }

    [Fact]
    public async Task QuorumApproval_OneReject_ReturnsToDraft()
    {
        // Setup: get to PendingApproval
        var created = await SetupToPendingApproval();

        // CISO rejects — should return to Draft
        var result = await _service.ProcessApprovalAsync(created.RequestId,
            new ApprovalDto { Decision = ApprovalDecision.Reject, Comments = "Security concerns" },
            "ciso", "ciso");
        Assert.Equal(RequestStatus.Draft, result.Status);
    }

    [Fact]
    public async Task IOCApproval_Approve_SetsPendingApproval()
    {
        // Setup: get to PendingIOCApproval
        var created = await SetupToPendingIOCApproval();

        var result = await _service.ProcessIOCApprovalAsync(created.RequestId,
            new IOCSubmitDto { Comments = "Looks good" }, "ioc");

        Assert.Equal(RequestStatus.PendingApproval, result.Status);
    }

    [Fact]
    public async Task ApprovalReject_FromPendingApproval_ReturnsToDraft()
    {
        var created = await SetupToPendingApproval();

        var rejected = await _service.ProcessApprovalAsync(created.RequestId,
            new ApprovalDto { Decision = ApprovalDecision.Reject, Comments = "Missing info" }, "ciso", "ciso");

        Assert.Equal(RequestStatus.Draft, rejected.Status);
    }

    [Fact]
    public async Task UpdateAsync_NonDraftRequest_Throws()
    {
        var created = await _service.CreateAsync(
            new CreateRequestDto { ApplicationName = "App", Environment = EnvironmentType.Production }, "admin");
        await _service.SubmitAsync(created.RequestId, "admin");

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.UpdateAsync(created.RequestId, new UpdateRequestDto { ProgrammingLanguage = "Go" }, "admin"));
    }

    [Fact]
    public async Task SendBack_FromPendingSysAdmin_GoesToDraft()
    {
        var created = await _service.CreateAsync(
            new CreateRequestDto { ApplicationName = "App", Environment = EnvironmentType.Production }, "admin");
        await _service.SubmitAsync(created.RequestId, "admin");

        var result = await _service.SendBackAsync(created.RequestId,
            new SendBackDto { Comments = "Missing application details" }, "sysadmin");

        Assert.Equal(RequestStatus.Draft, result.Status);
    }

    [Fact]
    public async Task SendBack_FromDataCenterReview_GoesToPendingSysAdmin()
    {
        var created = await _service.CreateAsync(
            new CreateRequestDto { ApplicationName = "App", Environment = EnvironmentType.Production }, "admin");
        await _service.SubmitAsync(created.RequestId, "admin");
        await _service.SubmitSysAdminAsync(created.RequestId,
            new SysAdminDetailsDto { SensitivityLevel = "Low", ServerResources = "Small", WebServer = "IIS", Hostname = "h1" }, "sa");

        var result = await _service.SendBackAsync(created.RequestId,
            new SendBackDto { Comments = "Wrong server resources selected" }, "dc-engineer");

        Assert.Equal(RequestStatus.PendingSysAdmin, result.Status);
    }

    [Fact]
    public async Task SendBack_FromPendingNOC_GoesToDataCenterReview()
    {
        var created = await _service.CreateAsync(
            new CreateRequestDto { ApplicationName = "App", Environment = EnvironmentType.Production }, "admin");
        await _service.SubmitAsync(created.RequestId, "admin");
        await _service.SubmitSysAdminAsync(created.RequestId,
            new SysAdminDetailsDto { SensitivityLevel = "Low", ServerResources = "Small", WebServer = "IIS", Hostname = "h1" }, "sa");
        await _service.SubmitDataCenterAsync(created.RequestId,
            new DataCenterDetailsDto { Environment = "Dell", UplinkSpeed = "1Gbps", BareMetalType = "VM", PortNumber = "80", DC = "DC1", RackRoom = "R1", RackNumber = "R01", Cluster = "HyperFlex" }, "dc");

        var result = await _service.SendBackAsync(created.RequestId,
            new SendBackDto { Comments = "Rack number incorrect" }, "noc-engineer");

        Assert.Equal(RequestStatus.DataCenterReview, result.Status);
    }

    [Fact]
    public async Task SendBack_FromPendingApproval_GoesToPendingIOCApproval()
    {
        var created = await SetupToPendingApproval();

        var result = await _service.SendBackAsync(created.RequestId,
            new SendBackDto { Comments = "Need additional security review" }, "ciso");

        Assert.Equal(RequestStatus.PendingIOCApproval, result.Status);
    }

    [Fact]
    public async Task SendBack_ThenCorrectAndResubmit_WorksEndToEnd()
    {
        // Create and submit
        var created = await _service.CreateAsync(
            new CreateRequestDto { ApplicationName = "App", Environment = EnvironmentType.Production }, "admin");
        await _service.SubmitAsync(created.RequestId, "admin");

        // SysAdmin sends back to Dev
        var sentBack = await _service.SendBackAsync(created.RequestId,
            new SendBackDto { Comments = "Please add DBMS info" }, "sysadmin");
        Assert.Equal(RequestStatus.Draft, sentBack.Status);

        // Dev resubmits
        var resubmitted = await _service.SubmitAsync(created.RequestId, "admin");
        Assert.Equal(RequestStatus.PendingSysAdmin, resubmitted.Status);

        // SysAdmin now approves by submitting details
        var sysAdmin = await _service.SubmitSysAdminAsync(created.RequestId,
            new SysAdminDetailsDto { SensitivityLevel = "High", ServerResources = "Medium", WebServer = "Nginx", Hostname = "h1" }, "sysadmin");
        Assert.Equal(RequestStatus.DataCenterReview, sysAdmin.Status);
    }

    [Fact]
    public async Task SendBack_FromDraft_Throws()
    {
        var created = await _service.CreateAsync(
            new CreateRequestDto { ApplicationName = "App", Environment = EnvironmentType.Production }, "admin");

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.SendBackAsync(created.RequestId, new SendBackDto { Comments = "test" }, "admin"));
    }

    [Fact]
    public async Task SendBack_RecordsCommentInStatusHistory()
    {
        var created = await _service.CreateAsync(
            new CreateRequestDto { ApplicationName = "App", Environment = EnvironmentType.Production }, "admin");
        await _service.SubmitAsync(created.RequestId, "admin");

        var result = await _service.SendBackAsync(created.RequestId,
            new SendBackDto { Comments = "Fix the hostname" }, "sysadmin");

        var history = result.StatusHistories.First();
        Assert.Contains("Sent back: Fix the hostname", history.Comments);
    }

    [Fact]
    public async Task SendBack_FromPendingApproval_GoesToPendingIOCApproval_ViaService()
    {
        var created = await SetupToPendingApproval();

        var result = await _service.SendBackAsync(created.RequestId,
            new SendBackDto { Comments = "NOC needs to fix IP" }, "ciso");

        Assert.Equal(RequestStatus.PendingIOCApproval, result.Status);
    }

    // Helper: setup a request all the way to PendingIOCApproval
    private async Task<RequestResponseDto> SetupToPendingIOCApproval()
    {
        var created = await _service.CreateAsync(
            new CreateRequestDto { ApplicationName = "App", Environment = EnvironmentType.Production }, "admin");
        await _service.SubmitAsync(created.RequestId, "admin");
        await _service.SubmitSysAdminAsync(created.RequestId,
            new SysAdminDetailsDto { SensitivityLevel = "Low", ServerResources = "Small", WebServer = "Apache", Hostname = "h1" }, "sa");
        await _service.SubmitDataCenterAsync(created.RequestId,
            new DataCenterDetailsDto { Environment = "Dell", UplinkSpeed = "1Gbps", BareMetalType = "VM", PortNumber = "80", DC = "DC1", RackRoom = "R1", RackNumber = "R01", Cluster = "HyperFlex" }, "dc");
        await _service.SubmitNOCAsync(created.RequestId,
            new NOCDetailsDto { IPAddress = "10.0.0.5", SubnetMask = "255.255.255.0", VLANID = "10", Gateway = "10.0.0.1", Port = "80", VIP = "10.0.0.100", FQDN = "app.local" }, "noc");
        await _service.SubmitSOCAsync(created.RequestId,
            new SOCDetailsDto { FirewallEntries = new List<FirewallEntryDto> { new() { PolicyName = "Allow-HTTP", VDOM = "root", Action = "Accept" } } }, "soc");
        return created;
    }

    // Helper: setup a request all the way to PendingApproval
    private async Task<RequestResponseDto> SetupToPendingApproval()
    {
        var created = await SetupToPendingIOCApproval();
        await _service.ProcessIOCApprovalAsync(created.RequestId,
            new IOCSubmitDto { Comments = "IOC approved" }, "ioc");
        return created;
    }
}
