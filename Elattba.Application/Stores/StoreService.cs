using ElAtaba.Domain.Entities;
using Elattba.Application.Auth;
using Elattba.Application.Common;
using Elattba.Core.DTOs;
using Elattba.Core.InterFaces;

namespace Elattba.Application.Stores;

public sealed class StoreService : IStoreService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService? _currentUserService;

    public StoreService(IUnitOfWork unitOfWork, ICurrentUserService? currentUserService = null)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<ServiceResult<IReadOnlyList<StoreDto>>> GetAllAsync()
    {
        try
        {
            var stores = await _unitOfWork.Stores.ListAsync(
                null,
                true,
                store => store.Owner!,
                store => store.Manager!,
                store => store.Category!,
                store => store.ProductLines);
            var data = stores.Select(store => store.ToStoreDto()).ToList();

            return new ServiceResult<IReadOnlyList<StoreDto>>(true, 200, "Stores retrieved successfully", data);
        }
        catch (Exception ex)
        {
            return Failure<IReadOnlyList<StoreDto>>(ex);
        }
    }

    public async Task<ServiceResult<StoreDto>> GetByIdAsync(int id)
    {
        try
        {
            var store = await GetStoreWithDetailsAsync(id, disableTracking: true);
            if (store == null)
            {
                return new ServiceResult<StoreDto>(false, 404, "Store not found.");
            }

            return new ServiceResult<StoreDto>(true, 200, "Store retrieved successfully", store.ToStoreDto());
        }
        catch (Exception ex)
        {
            return Failure<StoreDto>(ex);
        }
    }

    public async Task<ServiceResult<StoreDto>> CreateAsync(CreateStoreDto dto)
    {
        try
        {
            var owner = await _unitOfWork.Users.GetByIdAsync(dto.OwnerId);
            if (owner == null)
            {
                return new ServiceResult<StoreDto>(false, 404, "Owner not found.");
            }

            var ownerAuthorizationError = EnsureCurrentOwner(dto.OwnerId);
            if (ownerAuthorizationError != null)
            {
                return ownerAuthorizationError;
            }

            if (await _unitOfWork.Stores.AnyAsync(store => store.OwnerId == dto.OwnerId))
            {
                return new ServiceResult<StoreDto>(false, 400, "Owner already has a store.");
            }

            var managerResult = await GetManagerAsync(dto.ManagerId);
            if (!managerResult.Succeeded)
            {
                return new ServiceResult<StoreDto>(false, managerResult.StatusCode, managerResult.Message);
            }

            var category = await _unitOfWork.Categories.GetByIdAsync(dto.CategoryId);
            if (category == null)
            {
                return new ServiceResult<StoreDto>(false, 404, "Category not found.");
            }

            var productLines = new List<Category>();
            if (dto.ProductLineIds != null)
            {
                foreach (var catId in dto.ProductLineIds)
                {
                    var cat = await _unitOfWork.Categories.GetByIdAsync(catId);
                    if (cat != null) productLines.Add(cat);
                }
            }

            var store = new Store
            {
                OwnerId = dto.OwnerId,
                ManagerId = dto.ManagerId,
                CategoryId = dto.CategoryId,
                StoreName = dto.StoreName,
                Location = dto.Location,
                Description = dto.Description,
                ProductLines = productLines
            };

            await _unitOfWork.Stores.AddAsync(store);
            await _unitOfWork.CompleteAsync();

            store.Owner = owner;
            store.Manager = managerResult.Data;
            store.Category = category;
            return new ServiceResult<StoreDto>(true, 201, "Store created successfully", store.ToStoreDto());
        }
        catch (Exception ex)
        {
            return Failure<StoreDto>(ex);
        }
    }

    public async Task<ServiceResult<StoreDto>> UpdateAsync(int id, UpdateStoreDto dto)
    {
        try
        {
            var store = await GetStoreWithDetailsAsync(id, disableTracking: false);
            if (store == null)
            {
                return new ServiceResult<StoreDto>(false, 404, "Store not found.");
            }

            var storeAuthorizationError = EnsureCanManageStore(store);
            if (storeAuthorizationError != null)
            {
                return storeAuthorizationError;
            }

            var managerResult = await GetManagerAsync(dto.ManagerId);
            if (!managerResult.Succeeded)
            {
                return new ServiceResult<StoreDto>(false, managerResult.StatusCode, managerResult.Message);
            }

            var category = await _unitOfWork.Categories.GetByIdAsync(dto.CategoryId);
            if (category == null)
            {
                return new ServiceResult<StoreDto>(false, 404, "Category not found.");
            }

            store.ManagerId = dto.ManagerId;
            store.CategoryId = dto.CategoryId;
            store.StoreName = dto.StoreName;
            store.Location = dto.Location;
            store.Description = dto.Description;

            store.ProductLines.Clear();
            if (dto.ProductLineIds != null)
            {
                foreach (var catId in dto.ProductLineIds)
                {
                    var cat = await _unitOfWork.Categories.GetByIdAsync(catId);
                    if (cat != null) store.ProductLines.Add(cat);
                }
            }

            await _unitOfWork.Stores.UpdateAsync(store);
            await _unitOfWork.CompleteAsync();

            var updatedStore = await GetStoreWithDetailsAsync(id, disableTracking: true) ?? store;
            updatedStore.Manager = managerResult.Data;
            updatedStore.Category = category;
            return new ServiceResult<StoreDto>(true, 200, "Store updated successfully", updatedStore.ToStoreDto());
        }
        catch (Exception ex)
        {
            return Failure<StoreDto>(ex);
        }
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        try
        {
            var store = await _unitOfWork.Stores.GetByIdAsync(id);
            if (store == null)
            {
                return new ServiceResult(false, 404, "Store not found.");
            }

            await _unitOfWork.Stores.DeleteAsync(id);
            await _unitOfWork.CompleteAsync();

            return new ServiceResult(true, 200, "Store deleted successfully");
        }
        catch (Exception)
        {
            return new ServiceResult(false, 500, "Unexpected server error.");
        }
    }

    private async Task<ServiceResult<User?>> GetManagerAsync(int? managerId)
    {
        if (!managerId.HasValue)
        {
            return new ServiceResult<User?>(true, 200, "Manager not provided.");
        }

        var manager = await _unitOfWork.Users.GetByIdAsync(managerId.Value);
        return manager == null
            ? new ServiceResult<User?>(false, 404, "Manager not found.")
            : new ServiceResult<User?>(true, 200, "Manager retrieved successfully", manager);
    }

    private Task<Store?> GetStoreWithDetailsAsync(int id, bool disableTracking)
    {
        return _unitOfWork.Stores.GetFirstOrDefaultAsync(
            store => store.StoreId == id,
            disableTracking,
            store => store.Owner!,
            store => store.Manager!,
            store => store.Category!,
            store => store.ProductLines);
    }

    private static ServiceResult<T> Failure<T>(Exception ex) =>
        new(false, 500, "Unexpected server error.");

    private ServiceResult<StoreDto>? EnsureCurrentOwner(int ownerId)
    {
        if (_currentUserService?.IsAuthenticated != true || _currentUserService.Role == AuthConstants.AdminRole)
        {
            return null;
        }

        return _currentUserService.UserId == ownerId
            ? null
            : new ServiceResult<StoreDto>(false, 403, "You are not allowed to create a store for another owner.");
    }

    private ServiceResult<StoreDto>? EnsureCanManageStore(Store store)
    {
        if (_currentUserService?.IsAuthenticated != true || _currentUserService.Role == AuthConstants.AdminRole)
        {
            return null;
        }

        return _currentUserService.UserId == store.OwnerId ||
               _currentUserService.UserId == store.ManagerId ||
               _currentUserService.StoreId == store.StoreId
            ? null
            : new ServiceResult<StoreDto>(false, 403, "You are not allowed to manage this store.");
    }
}
