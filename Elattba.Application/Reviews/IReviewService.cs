using Elattba.Application.Common;
using Elattba.Core.DTOs;

namespace Elattba.Application.Reviews;

public interface IReviewService
{
    Task<ServiceResult<IReadOnlyList<ReviewDto>>> GetAllAsync();
    Task<ServiceResult<ReviewDto>> GetByIdAsync(int id);
    Task<ServiceResult<ReviewDto>> CreateAsync(CreateReviewDto dto);
    Task<ServiceResult<ReviewDto>> UpdateAsync(int id, UpdateReviewDto dto);
    Task<ServiceResult> DeleteAsync(int id);
}
