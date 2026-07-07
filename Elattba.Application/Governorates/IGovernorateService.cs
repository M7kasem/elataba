using Elattba.Application.Common;
using Elattba.Core.DTOs;

namespace Elattba.Application.Governorates;

public interface IGovernorateService
{
    Task<ServiceResult<IReadOnlyList<GovernorateDto>>> GetAllAsync();
    Task<ServiceResult<GovernorateDto>> GetByIdAsync(int id);
    Task<ServiceResult<GovernorateDto>> CreateAsync(CreateGovernorateDto dto);
    Task<ServiceResult<GovernorateDto>> UpdateAsync(int id, UpdateGovernorateDto dto);
    Task<ServiceResult> DeleteAsync(int id);
}
