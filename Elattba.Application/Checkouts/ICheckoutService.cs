using Elattba.Application.Common;
using Elattba.Core.DTOs;

namespace Elattba.Application.Checkouts;

public interface ICheckoutService
{
    Task<ServiceResult<CheckoutResultDto>> CreateAsync(CreateCheckoutDto dto);
}
