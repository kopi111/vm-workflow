using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VMWorkflow.Application.DTOs;
using VMWorkflow.Application.Interfaces;
using VMWorkflow.Domain.Entities;
using VMWorkflow.Domain.Enums;
using VMWorkflow.Domain.Services;
using VMWorkflow.Infrastructure.Data;

namespace VMWorkflow.Infrastructure.Services;

public class RequestService : IRequestService
{
    private readonly WorkflowDbContext _db;
    private readonly IWorkflowEngine _workflow;
    private readonly IScriptGenerationService _scriptService;
    private readonly ILogger<RequestService> _logger;

    public RequestService(WorkflowDbContext db, IWorkflowEngine workflow, IScriptGenerationService scriptService, ILogger<RequestService> logger)
    {
        _db = db;
        _workflow = workflow;
        _scriptService = scriptService;
        _logger = logger;
    }

    public async Task<RequestResponseDto> CreateAsync(CreateRequestDto dto, string createdBy)
    {
        // Retry loop to handle slug uniqueness constraint violations from concurrent requests
        const int maxRetries = 3;
        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            var sequenceNumber = await _db.Requests
                .CountAsync(r => r.ApplicationName == dto.ApplicationName && r.Environment == dto.Environment) + 1 + attempt;

            var slug = ObjectSlugGenerator.Generate(dto.ApplicationName, dto.Environment, sequenceNumber);

            var now = DateTime.UtcNow;
            var request = new Request
            {
                RequestId = Guid.NewGuid(),
                ApplicationName = dto.ApplicationName,
                Environment = dto.Environment,
                ObjectSlug = slug,
                ProgrammingLanguage = dto.ProgrammingLanguage,
                Framework = dto.Framework,
                Purpose = dto.Purpose,
                ExpectedUsers = dto.ExpectedUsers,
                DBMS = dto.DBMS,
                GitRepoLink = dto.GitRepoLink,
                AccessGroup = dto.AccessGroup,
                SLA = dto.SLA,
                FQDNSuggestion = dto.FQDNSuggestion,
                AuthenticationMethod = dto.AuthenticationMethod,
                Status = RequestStatus.Draft,
                CreatedBy = createdBy,
                CreatedAt = now,
                UpdatedAt = now
            };

            _db.Requests.Add(request);
            AddStatusHistory(request, RequestStatus.Draft, RequestStatus.Draft, createdBy, "Request created");

            try
            {
                await _db.SaveChangesAsync();
                _logger.LogInformation("Request {RequestId} created with slug {Slug} by {User}", request.RequestId, slug, createdBy);
                return MapToDto(request);
            }
            catch (DbUpdateException) when (attempt < maxRetries - 1)
            {
                // Unique constraint violation on ObjectSlug — detach and retry with next sequence
                _db.ChangeTracker.Clear();
                _logger.LogWarning("Slug collision on {Slug}, retrying (attempt {Attempt})", slug, attempt + 1);
            }
        }

        throw new InvalidOperationException("Failed to generate unique request slug after multiple attempts.");
    }

    public async Task<RequestResponseDto?> GetByIdAsync(Guid requestId)
    {
        var request = await GetRequestWithIncludes(requestId);
        return request == null ? null : MapToDto(request);
    }

    public async Task<List<RequestResponseDto>> GetAllAsync(string? createdBy = null)
    {
        var query = _db.Requests
            .Include(r => r.SysAdminDetails)
                .ThenInclude(s => s!.Services)
            .Include(r => r.DataCenterDetails)
            .Include(r => r.NOCDetails)
                .ThenInclude(n => n!.NetworkPaths)
            .Include(r => r.SOCDetails)
                .ThenInclude(s => s!.FirewallEntries)
                    .ThenInclude(f => f.Services)
            .Include(r => r.SOCDetails)
                .ThenInclude(s => s!.FirewallEntries)
                    .ThenInclude(f => f.SecurityProfiles)
                        .ThenInclude(sp => sp.SecurityProfile)
            .Include(r => r.StatusHistories)
            .AsQueryable();

        if (!string.IsNullOrEmpty(createdBy))
            query = query.Where(r => r.CreatedBy == createdBy);

        var requests = await query
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return requests.Select(MapToDto).ToList();
    }

    public async Task<RequestResponseDto> UpdateAsync(Guid requestId, UpdateRequestDto dto, string updatedBy)
    {
        var request = await GetRequestWithIncludes(requestId)
            ?? throw new KeyNotFoundException($"Request {requestId} not found.");

        if (request.Status != RequestStatus.Draft)
            throw new InvalidOperationException("Only Draft requests can be updated.");

        if (dto.ApplicationName != null) request.ApplicationName = dto.ApplicationName;
        if (dto.ProgrammingLanguage != null) request.ProgrammingLanguage = dto.ProgrammingLanguage;
        if (dto.Framework != null) request.Framework = dto.Framework;
        if (dto.Purpose != null) request.Purpose = dto.Purpose;
        if (dto.ExpectedUsers.HasValue) request.ExpectedUsers = dto.ExpectedUsers;
        if (dto.DBMS != null) request.DBMS = dto.DBMS;
        if (dto.GitRepoLink != null) request.GitRepoLink = dto.GitRepoLink;
        if (dto.AccessGroup != null) request.AccessGroup = dto.AccessGroup;
        if (dto.SLA.HasValue) request.SLA = dto.SLA.Value;
        if (dto.FQDNSuggestion != null) request.FQDNSuggestion = dto.FQDNSuggestion;
        if (dto.AuthenticationMethod != null) request.AuthenticationMethod = dto.AuthenticationMethod;

        request.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Request {RequestId} updated by {User}", requestId, updatedBy);
        return MapToDto(request);
    }

    public async Task<RequestResponseDto> SubmitAsync(Guid requestId, string submittedBy)
    {
        var request = await GetRequestWithIncludes(requestId)
            ?? throw new KeyNotFoundException($"Request {requestId} not found.");

        if (!_workflow.CanTransition(request.Status, RequestStatus.PendingSysAdmin))
            throw new InvalidOperationException($"Cannot submit request in {request.Status} status.");

        var oldStatus = request.Status;
        request.Status = RequestStatus.PendingSysAdmin;
        request.UpdatedAt = DateTime.UtcNow;
        AddStatusHistory(request, oldStatus, RequestStatus.PendingSysAdmin, submittedBy, "Request submitted");
        await _db.SaveChangesAsync();

        return MapToDto(request);
    }

    public async Task<RequestResponseDto> SubmitSysAdminAsync(Guid requestId, SysAdminDetailsDto dto, string submittedBy)
    {
        var request = await GetRequestWithIncludes(requestId)
            ?? throw new KeyNotFoundException($"Request {requestId} not found.");

        if (request.Status != RequestStatus.PendingSysAdmin)
            throw new InvalidOperationException($"Cannot submit SysAdmin details when status is {request.Status}.");

        var sysAdminDetails = new SysAdminDetails
        {
            SysAdminDetailsId = Guid.NewGuid(),
            RequestId = requestId,
            SensitivityLevel = dto.SensitivityLevel,
            ServerResources = string.IsNullOrWhiteSpace(dto.ServerResources) ? "Micro" : dto.ServerResources,
            WebServer = dto.WebServer,
            DatabaseNameType = dto.DatabaseNameType ?? "none",
            DatabaseName = string.IsNullOrWhiteSpace(dto.DatabaseName)
                ? $"{request.ApplicationName}-db"
                : dto.DatabaseName,
            DatabaseUsername = string.IsNullOrWhiteSpace(dto.DatabaseUsername)
                ? $"{request.ApplicationName}-user"
                : dto.DatabaseUsername,
            Hostname = dto.Hostname,
            SubmittedBy = submittedBy,
            SubmittedAt = DateTime.UtcNow
        };

        foreach (var svc in dto.Services)
        {
            sysAdminDetails.Services.Add(new ServiceEntry
            {
                ServiceEntryId = Guid.NewGuid(),
                SysAdminDetailsId = sysAdminDetails.SysAdminDetailsId,
                ServiceName = svc.ServiceName,
                Port = svc.Port,
                Protocol = svc.Protocol
            });
        }

        if (request.SysAdminDetails != null)
            _db.SysAdminDetails.Remove(request.SysAdminDetails);

        _db.SysAdminDetails.Add(sysAdminDetails);
        var oldStatus = request.Status;
        request.Status = RequestStatus.DataCenterReview;
        request.UpdatedAt = DateTime.UtcNow;
        AddStatusHistory(request, oldStatus, request.Status, submittedBy, "SysAdmin details submitted");
        await _db.SaveChangesAsync();

        _logger.LogInformation("SysAdmin details submitted for {RequestId} by {User}", requestId, submittedBy);
        return MapToDto(request);
    }

    public async Task<RequestResponseDto> SubmitDataCenterAsync(Guid requestId, DataCenterDetailsDto dto, string submittedBy)
    {
        var request = await GetRequestWithIncludes(requestId)
            ?? throw new KeyNotFoundException($"Request {requestId} not found.");

        if (request.Status != RequestStatus.DataCenterReview)
            throw new InvalidOperationException($"Cannot submit DC details when status is {request.Status}.");

        var dcDetails = new DataCenterDetails
        {
            DataCenterDetailsId = Guid.NewGuid(),
            RequestId = requestId,
            Environment = dto.Environment,
            UplinkSpeed = dto.UplinkSpeed,
            BareMetalType = dto.BareMetalType,
            PortNumber = dto.PortNumber,
            DC = dto.DC,
            RackRoom = dto.RackRoom,
            RackNumber = dto.RackNumber,
            Cluster = dto.Cluster,
            SubmittedBy = submittedBy,
            SubmittedAt = DateTime.UtcNow
        };

        if (request.DataCenterDetails != null)
            _db.DataCenterDetails.Remove(request.DataCenterDetails);

        _db.DataCenterDetails.Add(dcDetails);
        var oldStatus = request.Status;
        request.Status = RequestStatus.PendingNOC;
        request.UpdatedAt = DateTime.UtcNow;
        AddStatusHistory(request, oldStatus, request.Status, submittedBy, "Data center details submitted");
        await _db.SaveChangesAsync();

        _logger.LogInformation("DC details submitted for {RequestId} by {User}", requestId, submittedBy);
        return MapToDto(request);
    }

    public async Task<RequestResponseDto> SaveSysAdminAsync(Guid requestId, SysAdminDetailsDto dto, string savedBy)
    {
        var request = await GetRequestWithIncludes(requestId)
            ?? throw new KeyNotFoundException($"Request {requestId} not found.");

        if (request.Status != RequestStatus.PendingSysAdmin)
            throw new InvalidOperationException($"Cannot save SysAdmin details when status is {request.Status}.");

        var sysAdminDetails = new SysAdminDetails
        {
            SysAdminDetailsId = Guid.NewGuid(),
            RequestId = requestId,
            SensitivityLevel = dto.SensitivityLevel,
            ServerResources = string.IsNullOrWhiteSpace(dto.ServerResources) ? "Micro" : dto.ServerResources,
            WebServer = dto.WebServer,
            DatabaseNameType = dto.DatabaseNameType ?? "none",
            DatabaseName = string.IsNullOrWhiteSpace(dto.DatabaseName)
                ? $"{request.ApplicationName}-db"
                : dto.DatabaseName,
            DatabaseUsername = string.IsNullOrWhiteSpace(dto.DatabaseUsername)
                ? $"{request.ApplicationName}-user"
                : dto.DatabaseUsername,
            Hostname = dto.Hostname,
            SubmittedBy = savedBy,
            SubmittedAt = DateTime.UtcNow
        };

        foreach (var svc in dto.Services)
        {
            sysAdminDetails.Services.Add(new ServiceEntry
            {
                ServiceEntryId = Guid.NewGuid(),
                SysAdminDetailsId = sysAdminDetails.SysAdminDetailsId,
                ServiceName = svc.ServiceName,
                Port = svc.Port,
                Protocol = svc.Protocol
            });
        }

        if (request.SysAdminDetails != null)
            _db.SysAdminDetails.Remove(request.SysAdminDetails);

        _db.SysAdminDetails.Add(sysAdminDetails);
        request.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        _logger.LogInformation("SysAdmin details saved (without status change) for {RequestId} by {User}", requestId, savedBy);
        return MapToDto(request);
    }

    public async Task<RequestResponseDto> SaveDataCenterAsync(Guid requestId, DataCenterDetailsDto dto, string savedBy)
    {
        var request = await GetRequestWithIncludes(requestId)
            ?? throw new KeyNotFoundException($"Request {requestId} not found.");

        if (request.Status != RequestStatus.DataCenterReview)
            throw new InvalidOperationException($"Cannot save DC details when status is {request.Status}.");

        var dcDetails = new DataCenterDetails
        {
            DataCenterDetailsId = Guid.NewGuid(),
            RequestId = requestId,
            Environment = dto.Environment,
            UplinkSpeed = dto.UplinkSpeed,
            BareMetalType = dto.BareMetalType,
            PortNumber = dto.PortNumber,
            DC = dto.DC,
            RackRoom = dto.RackRoom,
            RackNumber = dto.RackNumber,
            Cluster = dto.Cluster,
            SubmittedBy = savedBy,
            SubmittedAt = DateTime.UtcNow
        };

        if (request.DataCenterDetails != null)
            _db.DataCenterDetails.Remove(request.DataCenterDetails);

        _db.DataCenterDetails.Add(dcDetails);
        request.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        _logger.LogInformation("DC details saved (without status change) for {RequestId} by {User}", requestId, savedBy);
        return MapToDto(request);
    }

    public async Task<RequestResponseDto> SubmitNOCAsync(Guid requestId, NOCDetailsDto dto, string submittedBy)
    {
        var request = await GetRequestWithIncludes(requestId)
            ?? throw new KeyNotFoundException($"Request {requestId} not found.");

        if (request.Status != RequestStatus.PendingNOC && request.Status != RequestStatus.PendingSOC && request.Status != RequestStatus.PendingIOCApproval)
            throw new InvalidOperationException($"Cannot submit NOC details when status is {request.Status}.");

        var nocDetails = new NOCDetails
        {
            NOCDetailsId = Guid.NewGuid(),
            RequestId = requestId,
            IPAddress = dto.IPAddress,
            SubnetMask = dto.SubnetMask,
            VLANID = dto.VLANID,
            Gateway = dto.Gateway,
            Port = dto.Port,
            VIP = dto.VIP,
            FQDN = dto.FQDN,
            VirtualIP = dto.VirtualIP,
            VirtualPort = dto.VirtualPort,
            VirtualFQDN = dto.VirtualFQDN,
            SubmittedBy = submittedBy,
            SubmittedAt = DateTime.UtcNow
        };

        foreach (var np in dto.NetworkPaths)
        {
            nocDetails.NetworkPaths.Add(new NetworkPathEntry
            {
                NetworkPathEntryId = Guid.NewGuid(),
                SwitchName = np.SwitchName,
                Port = np.Port,
                LinkSpeed = np.LinkSpeed
            });
        }

        if (request.NOCDetails != null)
            _db.NOCDetails.Remove(request.NOCDetails);

        _db.NOCDetails.Add(nocDetails);
        request.UpdatedAt = DateTime.UtcNow;

        var oldStatus = request.Status;
        if (oldStatus != RequestStatus.PendingIOCApproval)
        {
            var socCompleted = request.SOCDetails != null;
            if (_workflow.IsIOCReady(true, socCompleted))
            {
                request.Status = RequestStatus.PendingIOCApproval;
                AddStatusHistory(request, oldStatus, RequestStatus.PendingIOCApproval, submittedBy, "NOC submitted; both NOC+SOC complete — ready for IOC");
            }
            else
            {
                request.Status = RequestStatus.PendingSOC;
                AddStatusHistory(request, oldStatus, RequestStatus.PendingSOC, submittedBy, "NOC details submitted; awaiting SOC");
            }
        }
        else
        {
            AddStatusHistory(request, oldStatus, RequestStatus.PendingIOCApproval, submittedBy, "NOC details re-submitted by IOC Manager");
        }

        await _db.SaveChangesAsync();
        _logger.LogInformation("NOC details submitted for {RequestId} by {User}", requestId, submittedBy);
        return MapToDto(request);
    }

    public async Task<RequestResponseDto> SubmitSOCAsync(Guid requestId, SOCDetailsDto dto, string submittedBy)
    {
        var request = await GetRequestWithIncludes(requestId)
            ?? throw new KeyNotFoundException($"Request {requestId} not found.");

        if (request.Status != RequestStatus.PendingSOC && request.Status != RequestStatus.PendingNOC && request.Status != RequestStatus.PendingIOCApproval)
            throw new InvalidOperationException($"Cannot submit SOC details when status is {request.Status}.");

        var socDetails = new SOCDetails
        {
            SOCDetailsId = Guid.NewGuid(),
            RequestId = requestId,
            SubmittedBy = submittedBy,
            SubmittedAt = DateTime.UtcNow
        };

        foreach (var fe in dto.FirewallEntries)
        {
            var entry = new FirewallEntry
            {
                FirewallEntryId = Guid.NewGuid(),
                PolicyName = fe.PolicyName,
                VDOM = fe.VDOM,
                SourceInterface = fe.SourceInterface,
                DestinationInterface = fe.DestinationInterface,
                SourceIP = fe.SourceIP,
                DestinationIP = fe.DestinationIP,
                Schedule = fe.Schedule,
                Action = Enum.Parse<PolicyAction>(fe.Action, true)
            };

            // Add service entries
            foreach (var svc in fe.Services)
            {
                entry.Services.Add(new FirewallServiceEntry
                {
                    FirewallServiceEntryId = Guid.NewGuid(),
                    FirewallEntryId = entry.FirewallEntryId,
                    Port = svc.Port,
                    Protocol = svc.Protocol,
                    ServiceName = svc.ServiceName
                });
            }

            // Add security profile associations
            foreach (var profileId in fe.SecurityProfileIds)
            {
                entry.SecurityProfiles.Add(new FirewallEntrySecurityProfile
                {
                    FirewallEntryId = entry.FirewallEntryId,
                    SecurityProfileId = profileId
                });
            }

            socDetails.FirewallEntries.Add(entry);
        }

        if (request.SOCDetails != null)
            _db.SOCDetails.Remove(request.SOCDetails);

        _db.SOCDetails.Add(socDetails);
        request.UpdatedAt = DateTime.UtcNow;

        var oldStatus = request.Status;
        if (oldStatus != RequestStatus.PendingIOCApproval)
        {
            var nocCompleted = request.NOCDetails != null;
            if (_workflow.IsIOCReady(nocCompleted, true))
            {
                request.Status = RequestStatus.PendingIOCApproval;
                AddStatusHistory(request, oldStatus, RequestStatus.PendingIOCApproval, submittedBy, "SOC submitted; both NOC+SOC complete — ready for IOC");
            }
            else
            {
                request.Status = RequestStatus.PendingNOC;
                AddStatusHistory(request, oldStatus, RequestStatus.PendingNOC, submittedBy, "SOC details submitted; awaiting NOC");
            }
        }
        else
        {
            AddStatusHistory(request, oldStatus, RequestStatus.PendingIOCApproval, submittedBy, "SOC details re-submitted by IOC Manager");
        }

        await _db.SaveChangesAsync();
        _logger.LogInformation("SOC details submitted for {RequestId} by {User}", requestId, submittedBy);
        return MapToDto(request);
    }

    public async Task<RequestResponseDto> SaveNOCAsync(Guid requestId, NOCDetailsDto dto, string savedBy)
    {
        var request = await GetRequestWithIncludes(requestId)
            ?? throw new KeyNotFoundException($"Request {requestId} not found.");

        if (request.Status != RequestStatus.PendingNOC && request.Status != RequestStatus.PendingSOC)
            throw new InvalidOperationException($"Cannot save NOC details when status is {request.Status}.");

        var nocDetails = new NOCDetails
        {
            NOCDetailsId = Guid.NewGuid(),
            RequestId = requestId,
            IPAddress = dto.IPAddress,
            SubnetMask = dto.SubnetMask,
            VLANID = dto.VLANID,
            Gateway = dto.Gateway,
            Port = dto.Port,
            VIP = dto.VIP,
            FQDN = dto.FQDN,
            VirtualIP = dto.VirtualIP,
            VirtualPort = dto.VirtualPort,
            VirtualFQDN = dto.VirtualFQDN,
            SubmittedBy = savedBy,
            SubmittedAt = DateTime.UtcNow
        };

        foreach (var np in dto.NetworkPaths)
        {
            nocDetails.NetworkPaths.Add(new NetworkPathEntry
            {
                NetworkPathEntryId = Guid.NewGuid(),
                SwitchName = np.SwitchName,
                Port = np.Port,
                LinkSpeed = np.LinkSpeed
            });
        }

        if (request.NOCDetails != null)
            _db.NOCDetails.Remove(request.NOCDetails);

        _db.NOCDetails.Add(nocDetails);
        request.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        _logger.LogInformation("NOC details saved (without status change) for {RequestId} by {User}", requestId, savedBy);
        return MapToDto(request);
    }

    public async Task<RequestResponseDto> SaveSOCAsync(Guid requestId, SOCDetailsDto dto, string savedBy)
    {
        var request = await GetRequestWithIncludes(requestId)
            ?? throw new KeyNotFoundException($"Request {requestId} not found.");

        if (request.Status != RequestStatus.PendingSOC && request.Status != RequestStatus.PendingNOC)
            throw new InvalidOperationException($"Cannot save SOC details when status is {request.Status}.");

        var socDetails = new SOCDetails
        {
            SOCDetailsId = Guid.NewGuid(),
            RequestId = requestId,
            SubmittedBy = savedBy,
            SubmittedAt = DateTime.UtcNow
        };

        foreach (var fe in dto.FirewallEntries)
        {
            var entry = new FirewallEntry
            {
                FirewallEntryId = Guid.NewGuid(),
                PolicyName = fe.PolicyName,
                VDOM = fe.VDOM,
                SourceInterface = fe.SourceInterface,
                DestinationInterface = fe.DestinationInterface,
                SourceIP = fe.SourceIP,
                DestinationIP = fe.DestinationIP,
                Schedule = fe.Schedule,
                Action = Enum.Parse<PolicyAction>(fe.Action, true)
            };

            foreach (var svc in fe.Services)
            {
                entry.Services.Add(new FirewallServiceEntry
                {
                    FirewallServiceEntryId = Guid.NewGuid(),
                    FirewallEntryId = entry.FirewallEntryId,
                    Port = svc.Port,
                    Protocol = svc.Protocol,
                    ServiceName = svc.ServiceName
                });
            }

            foreach (var profileId in fe.SecurityProfileIds)
            {
                entry.SecurityProfiles.Add(new FirewallEntrySecurityProfile
                {
                    FirewallEntryId = entry.FirewallEntryId,
                    SecurityProfileId = profileId
                });
            }

            socDetails.FirewallEntries.Add(entry);
        }

        if (request.SOCDetails != null)
            _db.SOCDetails.Remove(request.SOCDetails);

        _db.SOCDetails.Add(socDetails);
        request.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        _logger.LogInformation("SOC details saved (without status change) for {RequestId} by {User}", requestId, savedBy);
        return MapToDto(request);
    }

    public async Task<RequestResponseDto> ProcessIOCApprovalAsync(Guid requestId, IOCSubmitDto dto, string approvedBy)
    {
        var request = await GetRequestWithIncludes(requestId)
            ?? throw new KeyNotFoundException($"Request {requestId} not found.");

        if (request.Status != RequestStatus.PendingIOCApproval)
            throw new InvalidOperationException($"Cannot process IOC approval when status is {request.Status}. Must be PendingIOCApproval.");

        var oldStatus = request.Status;
        request.IocComments = dto.Comments;
        request.Status = RequestStatus.PendingApproval;
        request.UpdatedAt = DateTime.UtcNow;
        AddStatusHistory(request, oldStatus, request.Status, approvedBy, dto.Comments ?? "IOC Manager submitted for approval");

        // Auto-generate FortiGate script on IOC approval
        try
        {
            var scriptContent = _scriptService.GenerateFortiGateScript(request);
            var script = new Script
            {
                ScriptId = Guid.NewGuid(),
                RequestId = request.RequestId,
                ScriptType = "FortiGate",
                Content = scriptContent,
                FileName = $"{request.ObjectSlug}-fortigate.conf",
                GeneratedBy = approvedBy,
                GeneratedAt = DateTime.UtcNow
            };
            _db.Scripts.Add(script);
            _logger.LogInformation("FortiGate script auto-generated for {RequestId} by {User}", requestId, approvedBy);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to auto-generate FortiGate script for {RequestId}", requestId);
        }

        await _db.SaveChangesAsync();

        _logger.LogInformation("IOC approval processed for {RequestId} by {User}", requestId, approvedBy);
        return MapToDto(request);
    }

    public async Task<RequestResponseDto> ProcessApprovalAsync(Guid requestId, ApprovalDto dto, string approvedBy, string role)
    {
        var request = await GetRequestWithIncludes(requestId)
            ?? throw new KeyNotFoundException($"Request {requestId} not found.");

        if (request.Status != RequestStatus.PendingApproval)
            throw new InvalidOperationException($"Cannot process approval when status is {request.Status}. Must be PendingApproval.");

        // Handle send-back (Return) — sends request back to IOC Manager
        if (dto.Decision == ApprovalDecision.Return)
        {
            var oldSt = request.Status;
            var previousStatus = _workflow.GetPreviousStatus(request.Status);
            request.Status = previousStatus;

            // Clear all approval decisions so approvers can re-vote after corrections
            request.CisoDecision = null;
            request.CisoComments = null;
            request.CisoApprovedBy = null;
            request.CisoApprovedAt = null;
            request.OpsDecision = null;
            request.OpsComments = null;
            request.OpsApprovedBy = null;
            request.OpsApprovedAt = null;

            request.UpdatedAt = DateTime.UtcNow;
            AddStatusHistory(request, oldSt, previousStatus, approvedBy, $"{role.ToUpper()} sent back: {dto.Comments}");
            await _db.SaveChangesAsync();

            _logger.LogInformation("{Role} sent back request {RequestId}: {Comments} by {User}", role, requestId, dto.Comments, approvedBy);
            return MapToDto(request);
        }

        var decision = dto.Decision == ApprovalDecision.Approve ? "Approved" : "Rejected";

        // Record the individual approval
        switch (role.ToLower())
        {
            case "ciso":
                request.CisoDecision = decision;
                request.CisoComments = dto.Comments;
                request.CisoApprovedBy = approvedBy;
                request.CisoApprovedAt = DateTime.UtcNow;
                break;
            case "ops":
                request.OpsDecision = decision;
                request.OpsComments = dto.Comments;
                request.OpsApprovedBy = approvedBy;
                request.OpsApprovedAt = DateTime.UtcNow;
                break;
            default:
                throw new ArgumentException($"Invalid approval role: {role}. Must be ciso or ops.");
        }

        var oldStatus = request.Status;

        // Check if any approver rejected
        if (_workflow.HasRejection(request.CisoDecision, request.OpsDecision))
        {
            request.Status = RequestStatus.Rejected;
            AddStatusHistory(request, oldStatus, request.Status, approvedBy, $"{role.ToUpper()} rejected: {dto.Comments}");
        }
        // Check if both CISO and Ops Manager approved
        else if (_workflow.HasFullApproval(request.CisoDecision, request.OpsDecision))
        {
            request.Status = RequestStatus.Approved;
            AddStatusHistory(request, oldStatus, request.Status, approvedBy, $"Both CISO and Ops Manager approved. {role.ToUpper()} decision: {decision}");
        }
        else
        {
            AddStatusHistory(request, oldStatus, oldStatus, approvedBy, $"{role.ToUpper()} decision: {decision}. Awaiting other approval.");
        }

        request.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        _logger.LogInformation("{Role} approval processed for {RequestId}: {Decision} by {User}", role, requestId, decision, approvedBy);
        return MapToDto(request);
    }

    public async Task<RequestResponseDto> SendBackAsync(Guid requestId, SendBackDto dto, string sentBackBy)
    {
        var request = await GetRequestWithIncludes(requestId)
            ?? throw new KeyNotFoundException($"Request {requestId} not found.");

        RequestStatus targetStatus;
        if (!string.IsNullOrWhiteSpace(dto.TargetStatus)
            && Enum.TryParse<RequestStatus>(dto.TargetStatus, out var parsed))
        {
            targetStatus = parsed;
        }
        else
        {
            targetStatus = _workflow.GetPreviousStatus(request.Status);
        }

        if (!_workflow.CanTransition(request.Status, targetStatus))
            throw new InvalidOperationException($"Cannot send back from {request.Status} to {targetStatus}.");

        var oldStatus = request.Status;
        request.Status = targetStatus;
        request.UpdatedAt = DateTime.UtcNow;
        AddStatusHistory(request, oldStatus, targetStatus, sentBackBy, $"Sent back: {dto.Comments}");
        await _db.SaveChangesAsync();

        _logger.LogInformation("Request {RequestId} sent back from {OldStatus} to {NewStatus} by {User}", requestId, oldStatus, targetStatus, sentBackBy);
        return MapToDto(request);
    }

    public async Task<RequestResponseDto> RejectAsync(Guid requestId, SendBackDto dto, string rejectedBy)
    {
        var request = await GetRequestWithIncludes(requestId)
            ?? throw new KeyNotFoundException($"Request {requestId} not found.");

        var oldStatus = request.Status;
        request.Status = RequestStatus.Rejected;
        request.UpdatedAt = DateTime.UtcNow;
        AddStatusHistory(request, oldStatus, RequestStatus.Rejected, rejectedBy, $"Rejected: {dto.Comments}");
        await _db.SaveChangesAsync();

        _logger.LogInformation("Request {RequestId} rejected from {OldStatus} by {User}", requestId, oldStatus, rejectedBy);
        return MapToDto(request);
    }

    public async Task<RequestResponseDto> UnrejectAsync(Guid requestId, string unrejectedBy)
    {
        var request = await GetRequestWithIncludes(requestId)
            ?? throw new KeyNotFoundException($"Request {requestId} not found.");

        if (request.Status != RequestStatus.Rejected)
            throw new InvalidOperationException("Only rejected requests can be unrejected.");

        request.Status = RequestStatus.PendingIOCApproval;
        request.UpdatedAt = DateTime.UtcNow;
        AddStatusHistory(request, RequestStatus.Rejected, RequestStatus.PendingIOCApproval, unrejectedBy, "Unrejected: restored to IOC Approval");
        await _db.SaveChangesAsync();

        _logger.LogInformation("Request {RequestId} unrejected by {User}", requestId, unrejectedBy);
        return MapToDto(request);
    }

    public async Task<List<RequestResponseDto>> GetRejectedByUserAsync(string username)
    {
        var rejectedRequestIds = await _db.StatusHistories
            .Where(h => h.ChangedBy == username && h.Comments != null && h.Comments.StartsWith("Rejected:"))
            .Select(h => h.RequestId)
            .Distinct()
            .ToListAsync();

        if (!rejectedRequestIds.Any())
            return new List<RequestResponseDto>();

        var requests = await _db.Requests
            .Include(r => r.SysAdminDetails).ThenInclude(s => s!.Services)
            .Include(r => r.DataCenterDetails)
            .Include(r => r.NOCDetails).ThenInclude(n => n!.NetworkPaths)
            .Include(r => r.SOCDetails).ThenInclude(s => s!.FirewallEntries).ThenInclude(f => f.Services)
            .Include(r => r.SOCDetails).ThenInclude(s => s!.FirewallEntries).ThenInclude(f => f.SecurityProfiles).ThenInclude(sp => sp.SecurityProfile)
            .Include(r => r.StatusHistories)
            .Where(r => rejectedRequestIds.Contains(r.RequestId) && r.Status == RequestStatus.Rejected)
            .OrderByDescending(r => r.UpdatedAt)
            .ToListAsync();

        return requests.Select(MapToDto).ToList();
    }

    public async Task DeleteAsync(Guid requestId, string deletedBy)
    {
        var request = await _db.Requests
            .Include(r => r.SysAdminDetails).ThenInclude(d => d!.Services)
            .Include(r => r.DataCenterDetails)
            .Include(r => r.NOCDetails).ThenInclude(d => d!.NetworkPaths)
            .Include(r => r.SOCDetails).ThenInclude(d => d!.FirewallEntries)!.ThenInclude(f => f.Services)
            .Include(r => r.SOCDetails).ThenInclude(d => d!.FirewallEntries)!.ThenInclude(f => f.SecurityProfiles)
            .Include(r => r.StatusHistories)
            .FirstOrDefaultAsync(r => r.RequestId == requestId)
            ?? throw new KeyNotFoundException($"Request {requestId} not found.");

        if (request.Status != RequestStatus.Draft)
            throw new InvalidOperationException("Only draft requests can be deleted.");

        _db.Requests.Remove(request);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Request {RequestId} deleted by {User}", requestId, deletedBy);
    }

    public async Task<List<RequestResponseDto>> GetPendingByRoleAsync(string role)
    {
        // NOC and SOC work in parallel — both should see requests at either PendingNOC or PendingSOC
        var targetStatuses = role.ToLower() switch
        {
            "sysadmin" => new[] { RequestStatus.PendingSysAdmin },
            "datacenter" => new[] { RequestStatus.DataCenterReview },
            "noc" => new[] { RequestStatus.PendingNOC, RequestStatus.PendingSOC },
            "soc" => new[] { RequestStatus.PendingNOC, RequestStatus.PendingSOC },
            "ioc" => new[] { RequestStatus.PendingNOC, RequestStatus.PendingSOC, RequestStatus.PendingIOCApproval },
            "ciso" or "ops" => new[] { RequestStatus.PendingApproval },
            _ => throw new ArgumentException($"Invalid role: {role}")
        };

        var query = _db.Requests
            .Include(r => r.SysAdminDetails)
                .ThenInclude(s => s!.Services)
            .Include(r => r.DataCenterDetails)
            .Include(r => r.NOCDetails)
                .ThenInclude(n => n!.NetworkPaths)
            .Include(r => r.SOCDetails)
                .ThenInclude(s => s!.FirewallEntries)
                    .ThenInclude(f => f.Services)
            .Include(r => r.SOCDetails)
                .ThenInclude(s => s!.FirewallEntries)
                    .ThenInclude(f => f.SecurityProfiles)
                        .ThenInclude(sp => sp.SecurityProfile)
            .Include(r => r.StatusHistories)
            .Where(r => targetStatuses.Contains(r.Status));

        // For approvers, filter out requests already decided by this role
        if (role.ToLower() == "ciso")
            query = query.Where(r => r.CisoDecision == null);
        else if (role.ToLower() == "ops")
            query = query.Where(r => r.OpsDecision == null);

        var requests = await query.OrderBy(r => r.CreatedAt).ToListAsync();
        return requests.Select(MapToDto).ToList();
    }

    public async Task<List<RequestResponseDto>> GetSentBackByUserAsync(string username)
    {
        // Find request IDs where this user performed a send-back (status history comments start with "Sent back:")
        var sentBackRequestIds = await _db.StatusHistories
            .Where(h => h.ChangedBy == username && h.Comments != null && h.Comments.StartsWith("Sent back:"))
            .Select(h => h.RequestId)
            .Distinct()
            .ToListAsync();

        if (!sentBackRequestIds.Any())
            return new List<RequestResponseDto>();

        var requests = await _db.Requests
            .Include(r => r.SysAdminDetails).ThenInclude(s => s!.Services)
            .Include(r => r.DataCenterDetails)
            .Include(r => r.NOCDetails).ThenInclude(n => n!.NetworkPaths)
            .Include(r => r.SOCDetails).ThenInclude(s => s!.FirewallEntries).ThenInclude(f => f.Services)
            .Include(r => r.SOCDetails).ThenInclude(s => s!.FirewallEntries).ThenInclude(f => f.SecurityProfiles).ThenInclude(sp => sp.SecurityProfile)
            .Include(r => r.StatusHistories)
            .Where(r => sentBackRequestIds.Contains(r.RequestId))
            .OrderByDescending(r => r.UpdatedAt)
            .ToListAsync();

        return requests.Select(MapToDto).ToList();
    }

    private void AddStatusHistory(Request request, RequestStatus oldStatus, RequestStatus newStatus, string changedBy, string? comments)
    {
        var history = new StatusHistory
        {
            StatusHistoryId = Guid.NewGuid(),
            RequestId = request.RequestId,
            OldStatus = oldStatus,
            NewStatus = newStatus,
            ChangedBy = changedBy,
            Comments = comments,
            Timestamp = DateTime.UtcNow
        };
        _db.StatusHistories.Add(history);
    }

    private async Task<Request?> GetRequestWithIncludes(Guid requestId)
    {
        return await _db.Requests
            .Include(r => r.SysAdminDetails)
                .ThenInclude(s => s!.Services)
            .Include(r => r.DataCenterDetails)
            .Include(r => r.NOCDetails)
                .ThenInclude(n => n!.NetworkPaths)
            .Include(r => r.SOCDetails)
                .ThenInclude(s => s!.FirewallEntries)
                    .ThenInclude(f => f.Services)
            .Include(r => r.SOCDetails)
                .ThenInclude(s => s!.FirewallEntries)
                    .ThenInclude(f => f.SecurityProfiles)
                        .ThenInclude(sp => sp.SecurityProfile)
            .Include(r => r.StatusHistories)
            .FirstOrDefaultAsync(r => r.RequestId == requestId);
    }

    private static RequestResponseDto MapToDto(Request r)
    {
        return new RequestResponseDto
        {
            RequestId = r.RequestId,
            ApplicationName = r.ApplicationName,
            Environment = r.Environment,
            ObjectSlug = r.ObjectSlug,
            ProgrammingLanguage = r.ProgrammingLanguage,
            Framework = r.Framework,
            Purpose = r.Purpose,
            ExpectedUsers = r.ExpectedUsers,
            DBMS = r.DBMS,
            GitRepoLink = r.GitRepoLink,
            AccessGroup = r.AccessGroup,
            SLA = r.SLA,
            FQDNSuggestion = r.FQDNSuggestion,
            AuthenticationMethod = r.AuthenticationMethod,
            Status = r.Status,
            ExternalSyncStatus = r.ExternalSyncStatus,
            NetBoxId = r.NetBoxId,
            FortiGatePolicyId = r.FortiGatePolicyId,
            IocComments = r.IocComments,
            CisoDecision = r.CisoDecision,
            CisoComments = r.CisoComments,
            CisoApprovedBy = r.CisoApprovedBy,
            OpsDecision = r.OpsDecision,
            OpsComments = r.OpsComments,
            OpsApprovedBy = r.OpsApprovedBy,
            CreatedBy = r.CreatedBy,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt,
            SysAdminDetails = r.SysAdminDetails == null ? null : new SysAdminDetailsDto
            {
                SensitivityLevel = r.SysAdminDetails.SensitivityLevel,
                ServerResources = r.SysAdminDetails.ServerResources,
                WebServer = r.SysAdminDetails.WebServer,
                DatabaseNameType = r.SysAdminDetails.DatabaseNameType,
                DatabaseName = r.SysAdminDetails.DatabaseName,
                DatabaseUsername = r.SysAdminDetails.DatabaseUsername,
                Hostname = r.SysAdminDetails.Hostname,
                Services = r.SysAdminDetails.Services.Select(s => new ServiceEntryDto
                {
                    ServiceName = s.ServiceName,
                    Port = s.Port,
                    Protocol = s.Protocol
                }).ToList()
            },
            DataCenterDetails = r.DataCenterDetails == null ? null : new DataCenterDetailsDto
            {
                Environment = r.DataCenterDetails.Environment,
                UplinkSpeed = r.DataCenterDetails.UplinkSpeed,
                BareMetalType = r.DataCenterDetails.BareMetalType,
                PortNumber = r.DataCenterDetails.PortNumber,
                DC = r.DataCenterDetails.DC,
                RackRoom = r.DataCenterDetails.RackRoom,
                RackNumber = r.DataCenterDetails.RackNumber,
                Cluster = r.DataCenterDetails.Cluster
            },
            NOCDetails = r.NOCDetails == null ? null : new NOCDetailsDto
            {
                IPAddress = r.NOCDetails.IPAddress,
                SubnetMask = r.NOCDetails.SubnetMask,
                VLANID = r.NOCDetails.VLANID,
                Gateway = r.NOCDetails.Gateway,
                Port = r.NOCDetails.Port,
                VIP = r.NOCDetails.VIP,
                FQDN = r.NOCDetails.FQDN,
                VirtualIP = r.NOCDetails.VirtualIP,
                VirtualPort = r.NOCDetails.VirtualPort,
                VirtualFQDN = r.NOCDetails.VirtualFQDN,
                NetworkPaths = r.NOCDetails.NetworkPaths.Select(np => new NetworkPathEntryDto
                {
                    SwitchName = np.SwitchName,
                    Port = np.Port,
                    LinkSpeed = np.LinkSpeed
                }).ToList()
            },
            SOCDetails = r.SOCDetails == null ? null : new SOCDetailsDto
            {
                FirewallEntries = r.SOCDetails.FirewallEntries.Select(fe => new FirewallEntryDto
                {
                    PolicyName = fe.PolicyName,
                    VDOM = fe.VDOM,
                    SourceInterface = fe.SourceInterface,
                    DestinationInterface = fe.DestinationInterface,
                    SourceIP = fe.SourceIP,
                    DestinationIP = fe.DestinationIP,
                    Services = fe.Services.Select(s => new FirewallServiceEntryDto
                    {
                        Port = s.Port,
                        Protocol = s.Protocol,
                        ServiceName = s.ServiceName
                    }).ToList(),
                    Schedule = fe.Schedule,
                    Action = fe.Action.ToString(),
                    SecurityProfileIds = fe.SecurityProfiles.Select(sp => sp.SecurityProfileId).ToList(),
                    SecurityProfileNames = fe.SecurityProfiles.Select(sp => sp.SecurityProfile.Name).ToList()
                }).ToList()
            },
            StatusHistories = r.StatusHistories.OrderByDescending(s => s.Timestamp).Select(s => new StatusHistoryDto
            {
                OldStatus = s.OldStatus,
                NewStatus = s.NewStatus,
                ChangedBy = s.ChangedBy,
                Comments = s.Comments,
                Timestamp = s.Timestamp
            }).ToList()
        };
    }
}
