using Elattba.Application.Common;
using Elattba.Core.DTOs;

namespace Elattba.Application.PricingTiers;

public interface IPricingTierService
{
    Task<ServiceResult<IReadOnlyList<PricingTierDto>>> GetAllAsync();
    Task<ServiceResult<PricingTierDto>> GetByIdAsync(int id);
    Task<ServiceResult<PricingTierDto>> CreateAsync(CreatePricingTierDto dto);
    Task<ServiceResult<PricingTierDto>> UpdateAsync(int id, UpdatePricingTierDto dto);
    Task<ServiceResult> DeleteAsync(int id);
}
