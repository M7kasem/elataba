using Elattba.Application.Common;
using Elattba.Core.DTOs;

namespace Elattba.Application.Carriers;

public interface ICarrierService
{
    Task<ServiceResult<IReadOnlyList<CarrierDto>>> GetAllAsync();
    Task<ServiceResult<CarrierDto>> GetByIdAsync(int id);
    Task<ServiceResult<CarrierDto>> CreateAsync(CreateCarrierDto dto);
    Task<ServiceResult<CarrierDto>> UpdateAsync(int id, UpdateCarrierDto dto);
    Task<ServiceResult> DeleteAsync(int id);
}
