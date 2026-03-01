# VM Workflow Automation Platform — Build Plan

**Date:** 2026-02-25
**Location:** ~/projects/vm-workflow/
**Stack:** ASP.NET Core 8 REST API + SQL Server + EF Core (Code First)
**Architecture:** Clean Architecture (Domain, Application, Infrastructure, API)

---

## Source Specs

- `AI_Full_Code_Generation_Build_Brief_VM_Workflow.docx`
- `VM_Workflow_Automation_Ready_Developer_Spec_v1.1.docx`

---

## Solution Structure

```
VMWorkflow/
├── VMWorkflow.sln
├── src/
│   ├── VMWorkflow.Domain/          # Entities, Enums, Interfaces
│   ├── VMWorkflow.Application/     # DTOs, Services, Interfaces, Validators
│   ├── VMWorkflow.Infrastructure/  # EF Core DbContext, Migrations, Stub Services
│   └── VMWorkflow.API/             # Controllers, Middleware, Program.cs
└── tests/
    └── VMWorkflow.Tests/           # Unit tests
```

---

## Current Progress

- [x] Solution skeleton created (sln + 5 projects + references)
- [x] Domain Enums: RequestStatus, ExternalSyncStatus, EnvironmentType
- [ ] Everything else below

---

## Phased Build Plan

### Phase 1 — Domain Layer (no blockers)

**1A: Domain Entities**
- `Request` — RequestId, ApplicationName, Environment, ObjectSlug (unique/immutable), DeviceRole, ExternalSyncStatus, NetBoxId, FortiGatePolicyId, TargetInfraEnv, OS, OSVersion, EstimatedUsers, InternetFacing, SpecialPorts, Status, CreatedBy, CreatedAt, UpdatedAt
- `DataCenterDetails` — CPU, Memory, StorageSize, ProvisioningType, DbRequired, DbType, DbPort
- `NOCDetails` — VLAN, Subnet, InternalIP, FQDN, DNSZone, InterfaceName, NetBoxSite, PublicIP, LoadBalancerVIP
- `SOCDetails` — SourceZone, DestinationZone, AllowedPorts, SecurityGroupName, EDRRequirement, LoggingSIEM, VulnScanWindow, PenTestSchedule
- `StatusHistory` — OldStatus, NewStatus, ChangedBy, Timestamp
- `AutomationLog` — RequestId, Action, Payload, Response, Timestamp
- `ApplicationDependency` — DependencyName, Port, Protocol

**1B: Domain Interfaces & Slug Logic**
- `INetBoxService` — CreateDeviceAsync, AssignIpAsync
- `IFortiGateService` — CreateAddressObjectAsync, CreateFirewallPolicyAsync
- `ObjectSlugGenerator` — generates immutable slug (e.g., `payroll-prod-01`)

---

### Phase 2 — Application Layer (blocked by Phase 1)

**2A: Application DTOs**
- CreateRequestDto, UpdateRequestDto, RequestResponseDto
- DataCenterDetailsDto, NOCDetailsDto, SOCDetailsDto
- ApprovalDto (Decision: Approve/Reject/Return + Comments)
- StatusHistoryDto, AutomationLogDto

**2B: Application Service Interfaces**
- `IRequestService` — Create, GetById, Update, SubmitDataCenter, SubmitNOC, SubmitSOC, GetAll
- `IWorkflowEngine` — TransitionStatus, CanTransition, IsIOCReady
- `IScriptGenerationService` — GenerateFortiGateScript

**2C: Application Services Implementation**
- `WorkflowEngine` — state machine: Draft → Submitted → DataCenterReview → PendingNOC/PendingSOC → PendingIOCApproval → Approved → Implemented → Closed. IOC gating: only when both NOC+SOC complete.
- `RequestService` — CRUD + workflow transitions + audit logging
- `FortiGateScriptGenerator` — CLI script from ObjectSlug, zones, IP
- Entity ↔ DTO mapping

---

### Phase 3 — Infrastructure Layer (blocked by Phase 1)

**3A: DbContext & Entity Configs**
- `WorkflowDbContext` with DbSets for all entities
- Fluent API configs (keys, indexes, relationships, ObjectSlug unique constraint)
- NuGet: EF Core, EF Core SqlServer, EF Core Design, EF Core Tools

**3B: Stub Services**
- `StubNetBoxService` — implements INetBoxService, logs actions
- `StubFortiGateService` — implements IFortiGateService, logs actions

---

### Phase 4 — API Layer (blocked by Phases 2 + 3)

**4A: Controllers**
| Method | Route | Role | Action |
|--------|-------|------|--------|
| POST | /api/requests | SysAdmin | Create request (Draft) |
| GET | /api/requests/{id} | All | Get request details |
| GET | /api/requests | All | List requests |
| PUT | /api/requests/{id} | SysAdmin | Update request |
| POST | /api/requests/{id}/datacenter | DCEngineer | Submit DC details |
| POST | /api/requests/{id}/noc | NOCEngineer | Submit NOC details |
| POST | /api/requests/{id}/soc | SOCEngineer | Submit SOC details |
| POST | /api/requests/{id}/approve | IOCManager | Approve/Reject/Return |
| GET | /api/requests/{id}/generate-script | All (post-approval) | Download FortiGate CLI |

**4B: Middleware, DI & Program.cs**
- Global exception handler middleware
- Audit logging middleware
- Program.cs: DI, Swagger, CORS, auth stub
- RBAC policy setup with dev-mode bypass

---

### Phase 5 — Unit Tests (blocked by Phase 2C)

- WorkflowEngineTests: state transitions, IOC gating (only after NOC+SOC)
- ObjectSlugGeneratorTests: slug format, immutability
- FortiGateScriptGeneratorTests: correct CLI output
- RequestServiceTests: CRUD, validation

---

### Phase 6 — Build & Verify (blocked by all above)

- EF Core initial migration
- `dotnet build` — all projects compile
- `dotnet test` — all tests pass
- `dotnet run --project src/VMWorkflow.API` — starts, Swagger accessible
- Seed data for dropdowns (environments, OS types, DB types)

---

## User Roles (RBAC)

1. **System Administrator / Application Owner** — Create/edit requests
2. **Data Center Engineer** — VM sizing, OS, DB config
3. **NOC Engineer** — VLAN, IP, FQDN, interface, NetBox
4. **SOC Engineer** — Firewall policies, zones, ports, scans
5. **IOC Manager** — Approve/Reject/Return
6. **Platform Administrator** — Manage dropdowns, roles, infra

## Workflow State Machine

```
Draft → Submitted → DataCenterReview → PendingNOC (parallel) → PendingIOCApproval → Approved → Implemented → Closed
                                      → PendingSOC (parallel) ↗
```

IOC Approval ONLY enabled when both NOC + SOC submissions are complete.

## Automation Hooks (Future-Ready)

- `INetBoxService` — CreateDevice, AssignIP (stub now, real later)
- `IFortiGateService` — CreateAddressObject, CreateFirewallPolicy (stub now, real later)
- FortiGate CLI script generation on approval

## Key Rules

- ObjectSlug is immutable once generated (e.g., `payroll-prod-01`)
- Full audit logging with immutable approval history
- StatusHistory tracks every transition with ChangedBy + Timestamp
- AutomationLog records all script generation and external sync attempts
