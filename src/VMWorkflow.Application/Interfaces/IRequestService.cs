using VMWorkflow.Application.DTOs;

namespace VMWorkflow.Application.Interfaces;

public interface IRequestService
{
    Task<RequestResponseDto> CreateAsync(CreateRequestDto dto, string createdBy);
    Task<RequestResponseDto?> GetByIdAsync(Guid requestId);
    Task<List<RequestResponseDto>> GetAllAsync(string? createdBy = null);
    Task<RequestResponseDto> UpdateAsync(Guid requestId, UpdateRequestDto dto, string updatedBy);
    Task<RequestResponseDto> SubmitAsync(Guid requestId, string submittedBy);
    Task<RequestResponseDto> SubmitSysAdminAsync(Guid requestId, SysAdminDetailsDto dto, string submittedBy);
    Task<RequestResponseDto> SaveSysAdminAsync(Guid requestId, SysAdminDetailsDto dto, string savedBy);
    Task<RequestResponseDto> SubmitDataCenterAsync(Guid requestId, DataCenterDetailsDto dto, string submittedBy);
    Task<RequestResponseDto> SaveDataCenterAsync(Guid requestId, DataCenterDetailsDto dto, string savedBy);
    Task<RequestResponseDto> SubmitNOCAsync(Guid requestId, NOCDetailsDto dto, string submittedBy);
    Task<RequestResponseDto> SaveNOCAsync(Guid requestId, NOCDetailsDto dto, string savedBy);
    Task<RequestResponseDto> SubmitSOCAsync(Guid requestId, SOCDetailsDto dto, string submittedBy);
    Task<RequestResponseDto> SaveSOCAsync(Guid requestId, SOCDetailsDto dto, string savedBy);
    Task<RequestResponseDto> ProcessIOCApprovalAsync(Guid requestId, IOCSubmitDto dto, string approvedBy);
    Task<RequestResponseDto> ProcessApprovalAsync(Guid requestId, ApprovalDto dto, string approvedBy, string role);
    Task<RequestResponseDto> SendBackAsync(Guid requestId, SendBackDto dto, string sentBackBy);
    Task<List<RequestResponseDto>> GetPendingByRoleAsync(string role);
}
