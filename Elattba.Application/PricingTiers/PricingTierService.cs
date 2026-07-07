using ElAtaba.Domain.Entities;
using Elattba.Application.Common;
using Elattba.Core.DTOs;
using Elattba.Core.InterFaces;

namespace Elattba.Application.PricingTiers;

public sealed class PricingTierService : IPricingTierService
{
    private readonly IUnitOfWork _unitOfWork;

    public PricingTierService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult<IReadOnlyList<PricingTierDto>>> GetAllAsync()
    {
        try
        {
            var tiers = await _unitOfWork.PricingTiers.GetAllAsync();
            var data = tiers.Select(tier => tier.ToDto()).ToList();
            return new ServiceResult<IReadOnlyList<PricingTierDto>>(true, 200, "Pricing tiers retrieved successfully", data);
        }
        catch (Exception ex)
        {
            return Failure<IReadOnlyList<PricingTierDto>>(ex);
        }
    }

    public async Task<ServiceResult<PricingTierDto>> GetByIdAsync(int id)
    {
        try
        {
            var tier = await _unitOfWork.PricingTiers.GetByIdAsync(id);
            return tier == null
                ? new ServiceResult<PricingTierDto>(false, 404, "Pricing tier not found.")
                : new ServiceResult<PricingTierDto>(true, 200, "Pricing tier retrieved successfully", tier.ToDto());
        }
        catch (Exception ex)
        {
            return Failure<PricingTierDto>(ex);
        }
    }

    public async Task<ServiceResult<PricingTierDto>> CreateAsync(CreatePricingTierDto dto)
    {
        try
        {
            var product = await _unitOfWork.Products.GetByIdAsync(dto.ProductId);
            if (product == null)
            {
                return new ServiceResult<PricingTierDto>(false, 404, "Product not found.");
            }

            var tier = new PricingTier
            {
                ProductId = dto.ProductId,
                MinQuantity = dto.MinQuantity,
                PricePerUnit = dto.PricePerUnit
            };

            await _unitOfWork.PricingTiers.AddAsync(tier);
            await _unitOfWork.CompleteAsync();

            return new ServiceResult<PricingTierDto>(true, 201, "Pricing tier created successfully", tier.ToDto());
        }
        catch (Exception ex)
        {
            return Failure<PricingTierDto>(ex);
        }
    }

    public async Task<ServiceResult<PricingTierDto>> UpdateAsync(int id, UpdatePricingTierDto dto)
    {
        try
        {
            var tier = await _unitOfWork.PricingTiers.GetByIdAsync(id);
            if (tier == null)
            {
                return new ServiceResult<PricingTierDto>(false, 404, "Pricing tier not found.");
            }

            tier.MinQuantity = dto.MinQuantity;
            tier.PricePerUnit = dto.PricePerUnit;

            await _unitOfWork.PricingTiers.UpdateAsync(tier);
            await _unitOfWork.CompleteAsync();

            return new ServiceResult<PricingTierDto>(true, 200, "Pricing tier updated successfully", tier.ToDto());
        }
        catch (Exception ex)
        {
            return Failure<PricingTierDto>(ex);
        }
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        try
        {
            var tier = await _unitOfWork.PricingTiers.GetByIdAsync(id);
            if (tier == null)
            {
                return new ServiceResult(false, 404, "Pricing tier not found.");
            }

            await _unitOfWork.PricingTiers.DeleteAsync(id);
            await _unitOfWork.CompleteAsync();

            return new ServiceResult(true, 200, "Pricing tier deleted successfully");
        }
        catch (Exception)
        {
            return new ServiceResult(false, 500, "Unexpected server error.");
        }
    }

    private static ServiceResult<T> Failure<T>(Exception ex) =>
        new(false, 500, "Unexpected server error.");
}
