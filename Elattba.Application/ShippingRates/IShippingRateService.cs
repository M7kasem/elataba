using Elattba.Application.Common;
using Elattba.Core.DTOs;

namespace Elattba.Application.ShippingRates;

public interface IShippingRateService
{
    Task<ServiceResult<IReadOnlyList<ShippingRateDto>>> GetAllAsync();
    Task<ServiceResult<ShippingRateDto>> GetByIdAsync(int id);
    Task<ServiceResult<ShippingRateDto>> CreateAsync(CreateShippingRateDto dto);
    Task<ServiceResult<ShippingRateDto>> UpdateAsync(int id, UpdateShippingRateDto dto);
    Task<ServiceResult> DeleteAsync(int id);
}
