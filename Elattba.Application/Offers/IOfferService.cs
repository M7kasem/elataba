using Elattba.Application.Common;
using Elattba.Core.DTOs;

namespace Elattba.Application.Offers;

public interface IOfferService
{
    Task<ServiceResult<IReadOnlyList<OfferDto>>> GetAllAsync();
    Task<ServiceResult<OfferDto>> GetByIdAsync(int id);
    Task<ServiceResult<OfferDto>> CreateAsync(CreateOfferDto dto);
    Task<ServiceResult<OfferDto>> UpdateAsync(int id, UpdateOfferDto dto);
    Task<ServiceResult> DeleteAsync(int id);
}
