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
    private readonly ILogger<RequestService> _logger;

    public RequestService(WorkflowDbContext db, IWorkflowEngine workflow, ILogger<RequestService> logger)
    {
        _db = db;
        _workflow = workflow;
        _logger = logger;
    }

    public async Task<RequestResponseDto> CreateAsync(CreateRequestDto dto, string createdBy)
    {
        var sequenceNumber = await _db.Requests
            .CountAsync(r => r.ApplicationName == dto.ApplicationName && r.Environment == dto.Environment) + 1;

        var slug = ObjectSlugGenerator.Generate(dto.ApplicationName, dto.Environment, sequenceNumber);

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
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Requests.Add(request);
        AddStatusHistory(request, RequestStatus.Draft, RequestStatus.Draft, createdBy, "Request created");
        await _db.SaveChangesAsync();

        _logger.LogInformation("Request {RequestId} created with slug {Slug} by {User}", request.RequestId, slug, createdBy);
        return MapToDto(request);
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
            .Include(r => r.DataCenterDetails)
            .Include(r => r.NOCDetails)
                .ThenInclude(n => n!.NetworkPaths)
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
            ServerResources = Enum.Parse<ServerResourceSize>(dto.ServerResources, true),
            WebServer = Enum.Parse<WebServerType>(dto.WebServer, true),
            DatabaseName = string.IsNullOrWhiteSpace(dto.DatabaseName)
                ? $"{request.ApplicationName}_Database"
                : dto.DatabaseName,
            DatabaseUsername = string.IsNullOrWhiteSpace(dto.DatabaseUsername)
                ? $"{request.ApplicationName}_User"
                : dto.DatabaseUsername,
            Hostname = dto.Hostname,
            SubmittedBy = submittedBy,
            SubmittedAt = DateTime.UtcNow
        };

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
            Environment = Enum.Parse<ServerEnvironment>(dto.Environment, true),
            UplinkSpeed = dto.UplinkSpeed,
            BareMetalType = Enum.Parse<BareMetalType>(dto.BareMetalType, true),
            PortNumber = dto.PortNumber,
            DC = dto.DC,
            RackRoom = dto.RackRoom,
            RackNumber = dto.RackNumber,
            Cluster = Enum.Parse<ClusterType>(dto.Cluster, true),
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

    public async Task<RequestResponseDto> SubmitNOCAsync(Guid requestId, NOCDetailsDto dto, string submittedBy)
    {
        var request = await GetRequestWithIncludes(requestId)
            ?? throw new KeyNotFoundException($"Request {requestId} not found.");

        if (request.Status != RequestStatus.PendingNOC && request.Status != RequestStatus.PendingSOC)
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

        await _db.SaveChangesAsync();
        _logger.LogInformation("NOC details submitted for {RequestId} by {User}", requestId, submittedBy);
        return MapToDto(request);
    }

    public async Task<RequestResponseDto> SubmitSOCAsync(Guid requestId, SOCDetailsDto dto, string submittedBy)
    {
        var request = await GetRequestWithIncludes(requestId)
            ?? throw new KeyNotFoundException($"Request {requestId} not found.");

        if (request.Status != RequestStatus.PendingSOC && request.Status != RequestStatus.PendingNOC)
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
                Services = fe.Services,
                Schedule = fe.Schedule,
                Action = Enum.Parse<PolicyAction>(fe.Action, true)
            };

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

        await _db.SaveChangesAsync();
        _logger.LogInformation("SOC details submitted for {RequestId} by {User}", requestId, submittedBy);
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
            case "cto":
                request.CtoDecision = decision;
                request.CtoComments = dto.Comments;
                request.CtoApprovedBy = approvedBy;
                request.CtoApprovedAt = DateTime.UtcNow;
                break;
            case "ops":
                request.OpsDecision = decision;
                request.OpsComments = dto.Comments;
                request.OpsApprovedBy = approvedBy;
                request.OpsApprovedAt = DateTime.UtcNow;
                break;
            default:
                throw new ArgumentException($"Invalid approval role: {role}. Must be ciso, cto, or ops.");
        }

        var oldStatus = request.Status;

        // Check if any approver rejected
        if (_workflow.HasRejection(request.CisoDecision, request.CtoDecision, request.OpsDecision))
        {
            request.Status = RequestStatus.Rejected;
            AddStatusHistory(request, oldStatus, request.Status, approvedBy, $"{role.ToUpper()} rejected: {dto.Comments}");
        }
        // Check if we have quorum (2 of 3 approved)
        else if (_workflow.HasQuorumApproval(request.CisoDecision, request.CtoDecision, request.OpsDecision))
        {
            request.Status = RequestStatus.Approved;
            AddStatusHistory(request, oldStatus, request.Status, approvedBy, $"Quorum reached (2 of 3 approved). {role.ToUpper()} decision: {decision}");
        }
        else
        {
            AddStatusHistory(request, oldStatus, oldStatus, approvedBy, $"{role.ToUpper()} decision: {decision}. Awaiting more approvals.");
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

        var previousStatus = _workflow.GetPreviousStatus(request.Status);

        if (!_workflow.CanTransition(request.Status, previousStatus))
            throw new InvalidOperationException($"Cannot send back from {request.Status}.");

        var oldStatus = request.Status;
        request.Status = previousStatus;
        request.UpdatedAt = DateTime.UtcNow;
        AddStatusHistory(request, oldStatus, previousStatus, sentBackBy, $"Sent back: {dto.Comments}");
        await _db.SaveChangesAsync();

        _logger.LogInformation("Request {RequestId} sent back from {OldStatus} to {NewStatus} by {User}", requestId, oldStatus, previousStatus, sentBackBy);
        return MapToDto(request);
    }

    public async Task<List<RequestResponseDto>> GetPendingByRoleAsync(string role)
    {
        var targetStatus = role.ToLower() switch
        {
            "sysadmin" => RequestStatus.PendingSysAdmin,
            "datacenter" => RequestStatus.DataCenterReview,
            "noc" => RequestStatus.PendingNOC,
            "soc" => RequestStatus.PendingSOC,
            "ioc" => RequestStatus.PendingIOCApproval,
            "ciso" or "cto" or "ops" => RequestStatus.PendingApproval,
            _ => throw new ArgumentException($"Invalid role: {role}")
        };

        var query = _db.Requests
            .Include(r => r.SysAdminDetails)
            .Include(r => r.DataCenterDetails)
            .Include(r => r.NOCDetails)
                .ThenInclude(n => n!.NetworkPaths)
            .Include(r => r.SOCDetails)
                .ThenInclude(s => s!.FirewallEntries)
                    .ThenInclude(f => f.SecurityProfiles)
                        .ThenInclude(sp => sp.SecurityProfile)
            .Include(r => r.StatusHistories)
            .Where(r => r.Status == targetStatus);

        // For approvers, filter out requests already decided by this role
        if (role.ToLower() == "ciso")
            query = query.Where(r => r.CisoDecision == null);
        else if (role.ToLower() == "cto")
            query = query.Where(r => r.CtoDecision == null);
        else if (role.ToLower() == "ops")
            query = query.Where(r => r.OpsDecision == null);

        var requests = await query.OrderBy(r => r.CreatedAt).ToListAsync();
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
            .Include(r => r.DataCenterDetails)
            .Include(r => r.NOCDetails)
                .ThenInclude(n => n!.NetworkPaths)
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
            CtoDecision = r.CtoDecision,
            CtoComments = r.CtoComments,
            CtoApprovedBy = r.CtoApprovedBy,
            OpsDecision = r.OpsDecision,
            OpsComments = r.OpsComments,
            OpsApprovedBy = r.OpsApprovedBy,
            CreatedBy = r.CreatedBy,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt,
            SysAdminDetails = r.SysAdminDetails == null ? null : new SysAdminDetailsDto
            {
                SensitivityLevel = r.SysAdminDetails.SensitivityLevel,
                ServerResources = r.SysAdminDetails.ServerResources.ToString(),
                WebServer = r.SysAdminDetails.WebServer.ToString(),
                DatabaseName = r.SysAdminDetails.DatabaseName,
                DatabaseUsername = r.SysAdminDetails.DatabaseUsername,
                Hostname = r.SysAdminDetails.Hostname
            },
            DataCenterDetails = r.DataCenterDetails == null ? null : new DataCenterDetailsDto
            {
                Environment = r.DataCenterDetails.Environment.ToString(),
                UplinkSpeed = r.DataCenterDetails.UplinkSpeed,
                BareMetalType = r.DataCenterDetails.BareMetalType.ToString(),
                PortNumber = r.DataCenterDetails.PortNumber,
                DC = r.DataCenterDetails.DC,
                RackRoom = r.DataCenterDetails.RackRoom,
                RackNumber = r.DataCenterDetails.RackNumber,
                Cluster = r.DataCenterDetails.Cluster.ToString()
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
                    Services = fe.Services,
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
