using ElAtaba.Domain.Entities;
using Elattba.Application.Common;
using Elattba.Core.DTOs;
using Elattba.Core.InterFaces;

namespace Elattba.Application.Carriers;

public sealed class CarrierService : ICarrierService
{
    private readonly IUnitOfWork _unitOfWork;

    public CarrierService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult<IReadOnlyList<CarrierDto>>> GetAllAsync()
    {
        try
        {
            var carriers = await _unitOfWork.Carriers.GetAllAsync();
            var data = carriers.Select(carrier => carrier.ToDto()).ToList();
            return new ServiceResult<IReadOnlyList<CarrierDto>>(true, 200, "Carriers retrieved successfully", data);
        }
        catch (Exception ex)
        {
            return Failure<IReadOnlyList<CarrierDto>>(ex);
        }
    }

    public async Task<ServiceResult<CarrierDto>> GetByIdAsync(int id)
    {
        try
        {
            var carrier = await _unitOfWork.Carriers.GetByIdAsync(id);
            return carrier == null
                ? new ServiceResult<CarrierDto>(false, 404, "Carrier not found.")
                : new ServiceResult<CarrierDto>(true, 200, "Carrier retrieved successfully", carrier.ToDto());
        }
        catch (Exception ex)
        {
            return Failure<CarrierDto>(ex);
        }
    }

    public async Task<ServiceResult<CarrierDto>> CreateAsync(CreateCarrierDto dto)
    {
        try
        {
            var carrier = new Carrier
            {
                Name = dto.Name,
                IsActive = dto.IsActive
            };

            await _unitOfWork.Carriers.AddAsync(carrier);
            await _unitOfWork.CompleteAsync();

            return new ServiceResult<CarrierDto>(true, 201, "Carrier created successfully", carrier.ToDto());
        }
        catch (Exception ex)
        {
            return Failure<CarrierDto>(ex);
        }
    }

    public async Task<ServiceResult<CarrierDto>> UpdateAsync(int id, UpdateCarrierDto dto)
    {
        try
        {
            var carrier = await _unitOfWork.Carriers.GetByIdAsync(id);
            if (carrier == null)
            {
                return new ServiceResult<CarrierDto>(false, 404, "Carrier not found.");
            }

            carrier.Name = dto.Name;
            carrier.IsActive = dto.IsActive;

            await _unitOfWork.Carriers.UpdateAsync(carrier);
            await _unitOfWork.CompleteAsync();

            return new ServiceResult<CarrierDto>(true, 200, "Carrier updated successfully", carrier.ToDto());
        }
        catch (Exception ex)
        {
            return Failure<CarrierDto>(ex);
        }
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        try
        {
            var carrier = await _unitOfWork.Carriers.GetByIdAsync(id);
            if (carrier == null)
            {
                return new ServiceResult(false, 404, "Carrier not found.");
            }

            await _unitOfWork.Carriers.DeleteAsync(id);
            await _unitOfWork.CompleteAsync();

            return new ServiceResult(true, 200, "Carrier deleted successfully");
        }
        catch (Exception)
        {
            return new ServiceResult(false, 500, "Unexpected server error.");
        }
    }

    private static ServiceResult<T> Failure<T>(Exception ex) =>
        new(false, 500, "Unexpected server error.");
}
