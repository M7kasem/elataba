using Elattba.Application.Common;
using Elattba.Core.DTOs;

namespace Elattba.Application.Stores;

public interface IStoreService
{
    Task<ServiceResult<IReadOnlyList<StoreDto>>> GetAllAsync();
    Task<ServiceResult<StoreDto>> GetByIdAsync(int id);
    Task<ServiceResult<StoreDto>> CreateAsync(CreateStoreDto dto);
    Task<ServiceResult<StoreDto>> UpdateAsync(int id, UpdateStoreDto dto);
    Task<ServiceResult> DeleteAsync(int id);
}
