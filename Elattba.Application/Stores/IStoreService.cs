using Elattba.Application.Common;
using Elattba.Core.DTOs;

namespace Elattba.Application.Stores;

public interface IStoreService
{
    Task<ServiceResult<Pagination<StoreDto>>> GetAllAsync(StoreParams storeParams);
    Task<ServiceResult<StoreDto>> GetByIdAsync(int id);
    Task<ServiceResult<StoreDto>> CreateAsync(CreateStoreDto dto);
    Task<ServiceResult<StoreDto>> UpdateAsync(int id, UpdateStoreDto dto);
    Task<ServiceResult> DeleteAsync(int id);
    Task<ServiceResult<string>> UploadLogoAsync(int storeId, Elattba.Core.Services.ImageUploadFile file);
}
