using ElAtaba.Domain.Entities;
using Elattba.Application.Auth;
using Elattba.Application.Common;
using Elattba.Core.DTOs;
using Elattba.Core.InterFaces;

namespace Elattba.Application.Offers;

public sealed class OfferService : IOfferService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService? _currentUserService;

    public OfferService(IUnitOfWork unitOfWork, ICurrentUserService? currentUserService = null)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<ServiceResult<IReadOnlyList<OfferDto>>> GetAllAsync()
    {
        try
        {
            var offers = await GetOffersWithDetailsAsync();
            var data = offers.Select(offer => offer.ToOfferDto()).ToList();

            return new ServiceResult<IReadOnlyList<OfferDto>>(true, 200, "Offers retrieved successfully", data);
        }
        catch (Exception ex)
        {
            return Failure<IReadOnlyList<OfferDto>>(ex);
        }
    }

    public async Task<ServiceResult<OfferDto>> GetByIdAsync(int id)
    {
        try
        {
            var offer = await GetOfferWithDetailsAsync(id);
            if (offer == null)
            {
                return new ServiceResult<OfferDto>(false, 404, "Offer not found.");
            }

            return new ServiceResult<OfferDto>(true, 200, "Offer retrieved successfully", offer.ToOfferDto());
        }
        catch (Exception ex)
        {
            return Failure<OfferDto>(ex);
        }
    }

    public async Task<ServiceResult<OfferDto>> CreateAsync(CreateOfferDto dto)
    {
        try
        {
            var productIds = OfferBusinessRules.GetDistinctProductIds(dto.ProductIds);
            var validationError = OfferBusinessRules.ValidateOffer<OfferDto>(
                dto.DiscountPercentage,
                dto.StartDate,
                dto.EndDate,
                dto.AppliesToAllProducts,
                productIds);
            if (validationError != null)
            {
                return validationError;
            }

            var store = await _unitOfWork.Stores.GetByIdAsync(dto.StoreId);
            if (store == null)
            {
                return new ServiceResult<OfferDto>(false, 404, "Store not found.");
            }

            var authorizationError = EnsureCanManageStore(dto.StoreId);
            if (authorizationError != null)
            {
                return authorizationError;
            }

            var productValidationError = await ValidateProductsBelongToStoreAsync(productIds, dto.StoreId);
            if (productValidationError != null)
            {
                return productValidationError;
            }

            var overlapError = await ValidateNoOverlappingOfferAsync(
                dto.StoreId,
                dto.StartDate,
                dto.EndDate,
                dto.AppliesToAllProducts,
                productIds);
            if (overlapError != null)
            {
                return overlapError;
            }

            var offer = new Offer
            {
                StoreId = dto.StoreId,
                DiscountPercentage = dto.DiscountPercentage,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                AppliesToAllProducts = dto.AppliesToAllProducts
            };

            foreach (var productId in productIds)
            {
                offer.OfferProducts.Add(new OfferProduct
                {
                    ProductId = productId
                });
            }

            await _unitOfWork.Offers.AddAsync(offer);
            await _unitOfWork.CompleteAsync();

            offer.Store = store;
            return new ServiceResult<OfferDto>(true, 201, "Offer created successfully", offer.ToOfferDto());
        }
        catch (Exception ex)
        {
            return Failure<OfferDto>(ex);
        }
    }

    public async Task<ServiceResult<OfferDto>> UpdateAsync(int id, UpdateOfferDto dto)
    {
        try
        {
            var offer = await GetOfferWithDetailsForUpdateAsync(id);
            if (offer == null)
            {
                return new ServiceResult<OfferDto>(false, 404, "Offer not found.");
            }

            var authorizationError = EnsureCanManageStore(offer.StoreId);
            if (authorizationError != null)
            {
                return authorizationError;
            }

            var productIds = OfferBusinessRules.GetDistinctProductIds(dto.ProductIds);
            var validationError = OfferBusinessRules.ValidateOffer<OfferDto>(
                dto.DiscountPercentage,
                dto.StartDate,
                dto.EndDate,
                dto.AppliesToAllProducts,
                productIds);
            if (validationError != null)
            {
                return validationError;
            }

            var productValidationError = await ValidateProductsBelongToStoreAsync(productIds, offer.StoreId);
            if (productValidationError != null)
            {
                return productValidationError;
            }

            var overlapError = await ValidateNoOverlappingOfferAsync(
                offer.StoreId,
                dto.StartDate,
                dto.EndDate,
                dto.AppliesToAllProducts,
                productIds,
                offer.OfferId);
            if (overlapError != null)
            {
                return overlapError;
            }

            offer.DiscountPercentage = dto.DiscountPercentage;
            offer.StartDate = dto.StartDate;
            offer.EndDate = dto.EndDate;
            offer.AppliesToAllProducts = dto.AppliesToAllProducts;
            offer.OfferProducts.Clear();

            foreach (var productId in productIds)
            {
                offer.OfferProducts.Add(new OfferProduct
                {
                    OfferId = offer.OfferId,
                    ProductId = productId
                });
            }

            await _unitOfWork.Offers.UpdateAsync(offer);
            await _unitOfWork.CompleteAsync();

            return new ServiceResult<OfferDto>(true, 200, "Offer updated successfully", offer.ToOfferDto());
        }
        catch (Exception ex)
        {
            return Failure<OfferDto>(ex);
        }
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        try
        {
            var offer = await _unitOfWork.Offers.GetByIdAsync(id);
            if (offer == null)
            {
                return new ServiceResult(false, 404, "Offer not found.");
            }

            var authorizationError = EnsureCanManageStore(offer.StoreId);
            if (authorizationError != null)
            {
                return authorizationError;
            }

            await _unitOfWork.Offers.DeleteAsync(id);
            await _unitOfWork.CompleteAsync();

            return new ServiceResult(true, 200, "Offer deleted successfully");
        }
        catch (Exception)
        {
            return new ServiceResult(false, 500, "Unexpected server error.");
        }
    }

    private Task<IReadOnlyList<Offer>> GetOffersWithDetailsAsync()
    {
        return _unitOfWork.Offers.ListAsync(
            null,
            true,
            offer => offer.Store!,
            offer => offer.OfferProducts);
    }

    private Task<Offer?> GetOfferWithDetailsAsync(int id)
    {
        return _unitOfWork.Offers.GetFirstOrDefaultAsync(
            offer => offer.OfferId == id,
            true,
            offer => offer.Store!,
            offer => offer.OfferProducts);
    }

    private Task<Offer?> GetOfferWithDetailsForUpdateAsync(int id)
    {
        return _unitOfWork.Offers.GetFirstOrDefaultAsync(
            offer => offer.OfferId == id,
            false,
            offer => offer.Store!,
            offer => offer.OfferProducts);
    }

    private async Task<ServiceResult<OfferDto>?> ValidateProductsBelongToStoreAsync(
        IReadOnlyList<int> productIds,
        int storeId)
    {
        foreach (var productId in productIds)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product == null)
            {
                return new ServiceResult<OfferDto>(false, 404, $"Product {productId} not found.");
            }

            if (product.StoreId != storeId)
            {
                return new ServiceResult<OfferDto>(false, 400, $"Product {productId} does not belong to store {storeId}.");
            }
        }

        return null;
    }

    private async Task<ServiceResult<OfferDto>?> ValidateNoOverlappingOfferAsync(
        int storeId,
        DateTime startDate,
        DateTime endDate,
        bool appliesToAllProducts,
        IReadOnlyList<int> productIds,
        int? excludedOfferId = null)
    {
        var existingOffers = await _unitOfWork.Offers.ListAsync(
            offer => offer.StoreId == storeId && offer.StartDate < endDate && startDate < offer.EndDate,
            true,
            offer => offer.OfferProducts);

        var overlappingOffer = OfferBusinessRules.FindOverlappingOffer(
            existingOffers,
            startDate,
            endDate,
            appliesToAllProducts,
            productIds,
            excludedOfferId);

        return overlappingOffer == null
            ? null
            : new ServiceResult<OfferDto>(false, 400, OfferBusinessRules.BuildOverlapMessage(overlappingOffer));
    }

    private static ServiceResult<T> Failure<T>(Exception ex) =>
        new(false, 500, "Unexpected server error.");

    private ServiceResult<OfferDto>? EnsureCanManageStore(int storeId)
    {
        if (_currentUserService?.IsAuthenticated != true || _currentUserService.Role == AuthConstants.AdminRole)
        {
            return null;
        }

        return _currentUserService.StoreId == storeId
            ? null
            : new ServiceResult<OfferDto>(false, 403, "You are not allowed to manage this store.");
    }
}
