using Elattba.Application.Common;
using Elattba.Core.DTOs;

namespace Elattba.Application.Categories;

public interface ICategoryService
{
    Task<ServiceResult<IReadOnlyList<CategoryDto>>> GetAllAsync();
    Task<ServiceResult<CategoryDto>> GetByIdAsync(int id);
    Task<ServiceResult<CategoryDto>> CreateAsync(CreateCategoryDto dto);
    Task<ServiceResult<CategoryDto>> UpdateAsync(int id, UpdateCategoryDto dto);
    Task<ServiceResult> DeleteAsync(int id);
}
