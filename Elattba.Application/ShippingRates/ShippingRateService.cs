using ElAtaba.Domain.Entities;
using Elattba.Application.Common;
using Elattba.Core.DTOs;
using Elattba.Core.InterFaces;

namespace Elattba.Application.ShippingRates;

public sealed class ShippingRateService : IShippingRateService
{
    private readonly IUnitOfWork _unitOfWork;

    public ShippingRateService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult<IReadOnlyList<ShippingRateDto>>> GetAllAsync()
    {
        try
        {
            var rates = await _unitOfWork.ShippingRates.ListAsync(
                null,
                true,
                rate => rate.Carrier!,
                rate => rate.Governorate!);
            var data = rates.Select(rate => rate.ToShippingRateDto()).ToList();

            return new ServiceResult<IReadOnlyList<ShippingRateDto>>(true, 200, "Shipping rates retrieved successfully", data);
        }
        catch (Exception ex)
        {
            return Failure<IReadOnlyList<ShippingRateDto>>(ex);
        }
    }

    public async Task<ServiceResult<ShippingRateDto>> GetByIdAsync(int id)
    {
        try
        {
            var rate = await GetRateWithDetailsAsync(id, disableTracking: true);
            if (rate == null)
            {
                return new ServiceResult<ShippingRateDto>(false, 404, "Shipping rate not found.");
            }

            return new ServiceResult<ShippingRateDto>(true, 200, "Shipping rate retrieved successfully", rate.ToShippingRateDto());
        }
        catch (Exception ex)
        {
            return Failure<ShippingRateDto>(ex);
        }
    }

    public async Task<ServiceResult<ShippingRateDto>> CreateAsync(CreateShippingRateDto dto)
    {
        try
        {
            var carrier = await _unitOfWork.Carriers.GetByIdAsync(dto.CarrierId);
            if (carrier == null)
            {
                return new ServiceResult<ShippingRateDto>(false, 404, "Carrier not found.");
            }

            var governorate = await _unitOfWork.Governorates.GetByIdAsync(dto.GovernorateId);
            if (governorate == null)
            {
                return new ServiceResult<ShippingRateDto>(false, 404, "Governorate not found.");
            }

            if (await _unitOfWork.ShippingRates.AnyAsync(rate =>
                rate.CarrierId == dto.CarrierId && rate.GovernorateId == dto.GovernorateId))
            {
                return new ServiceResult<ShippingRateDto>(false, 400, "Shipping rate already exists for this carrier and governorate.");
            }

            var rate = new ShippingRate
            {
                CarrierId = dto.CarrierId,
                GovernorateId = dto.GovernorateId,
                Cost = dto.Cost
            };

            await _unitOfWork.ShippingRates.AddAsync(rate);
            await _unitOfWork.CompleteAsync();

            rate.Carrier = carrier;
            rate.Governorate = governorate;
            return new ServiceResult<ShippingRateDto>(true, 201, "Shipping rate created successfully", rate.ToShippingRateDto());
        }
        catch (Exception ex)
        {
            return Failure<ShippingRateDto>(ex);
        }
    }

    public async Task<ServiceResult<ShippingRateDto>> UpdateAsync(int id, UpdateShippingRateDto dto)
    {
        try
        {
            var rate = await _unitOfWork.ShippingRates.GetByIdAsync(id);
            if (rate == null)
            {
                return new ServiceResult<ShippingRateDto>(false, 404, "Shipping rate not found.");
            }

            rate.Cost = dto.Cost;

            await _unitOfWork.ShippingRates.UpdateAsync(rate);
            await _unitOfWork.CompleteAsync();

            var updatedRate = await GetRateWithDetailsAsync(id, disableTracking: true) ?? rate;
            return new ServiceResult<ShippingRateDto>(true, 200, "Shipping rate updated successfully", updatedRate.ToShippingRateDto());
        }
        catch (Exception ex)
        {
            return Failure<ShippingRateDto>(ex);
        }
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        try
        {
            var rate = await _unitOfWork.ShippingRates.GetByIdAsync(id);
            if (rate == null)
            {
                return new ServiceResult(false, 404, "Shipping rate not found.");
            }

            await _unitOfWork.ShippingRates.DeleteAsync(id);
            await _unitOfWork.CompleteAsync();

            return new ServiceResult(true, 200, "Shipping rate deleted successfully");
        }
        catch (Exception)
        {
            return new ServiceResult(false, 500, "Unexpected server error.");
        }
    }

    private Task<ShippingRate?> GetRateWithDetailsAsync(int id, bool disableTracking)
    {
        return _unitOfWork.ShippingRates.GetFirstOrDefaultAsync(
            rate => rate.ShippingRateId == id,
            disableTracking,
            rate => rate.Carrier!,
            rate => rate.Governorate!);
    }

    private static ServiceResult<T> Failure<T>(Exception ex) =>
        new(false, 500, "Unexpected server error.");
}
