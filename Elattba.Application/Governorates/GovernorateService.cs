using ElAtaba.Domain.Entities;
using Elattba.Application.Common;
using Elattba.Core.DTOs;
using Elattba.Core.InterFaces;

namespace Elattba.Application.Governorates;

public sealed class GovernorateService : IGovernorateService
{
    private readonly IUnitOfWork _unitOfWork;

    public GovernorateService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult<IReadOnlyList<GovernorateDto>>> GetAllAsync()
    {
        try
        {
            var governorates = await _unitOfWork.Governorates.GetAllAsync();
            var data = governorates.Select(governorate => governorate.ToDto()).ToList();
            return new ServiceResult<IReadOnlyList<GovernorateDto>>(true, 200, "Governorates retrieved successfully", data);
        }
        catch (Exception ex)
        {
            return Failure<IReadOnlyList<GovernorateDto>>(ex);
        }
    }

    public async Task<ServiceResult<GovernorateDto>> GetByIdAsync(int id)
    {
        try
        {
            var governorate = await _unitOfWork.Governorates.GetByIdAsync(id);
            return governorate == null
                ? new ServiceResult<GovernorateDto>(false, 404, "Governorate not found.")
                : new ServiceResult<GovernorateDto>(true, 200, "Governorate retrieved successfully", governorate.ToDto());
        }
        catch (Exception ex)
        {
            return Failure<GovernorateDto>(ex);
        }
    }

    public async Task<ServiceResult<GovernorateDto>> CreateAsync(CreateGovernorateDto dto)
    {
        try
        {
            var governorate = new Governorate
            {
                Name = dto.Name
            };

            await _unitOfWork.Governorates.AddAsync(governorate);
            await _unitOfWork.CompleteAsync();

            return new ServiceResult<GovernorateDto>(true, 201, "Governorate created successfully", governorate.ToDto());
        }
        catch (Exception ex)
        {
            return Failure<GovernorateDto>(ex);
        }
    }

    public async Task<ServiceResult<GovernorateDto>> UpdateAsync(int id, UpdateGovernorateDto dto)
    {
        try
        {
            var governorate = await _unitOfWork.Governorates.GetByIdAsync(id);
            if (governorate == null)
            {
                return new ServiceResult<GovernorateDto>(false, 404, "Governorate not found.");
            }

            governorate.Name = dto.Name;

            await _unitOfWork.Governorates.UpdateAsync(governorate);
            await _unitOfWork.CompleteAsync();

            return new ServiceResult<GovernorateDto>(true, 200, "Governorate updated successfully", governorate.ToDto());
        }
        catch (Exception ex)
        {
            return Failure<GovernorateDto>(ex);
        }
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        try
        {
            var governorate = await _unitOfWork.Governorates.GetByIdAsync(id);
            if (governorate == null)
            {
                return new ServiceResult(false, 404, "Governorate not found.");
            }

            await _unitOfWork.Governorates.DeleteAsync(id);
            await _unitOfWork.CompleteAsync();

            return new ServiceResult(true, 200, "Governorate deleted successfully");
        }
        catch (Exception)
        {
            return new ServiceResult(false, 500, "Unexpected server error.");
        }
    }

    private static ServiceResult<T> Failure<T>(Exception ex) =>
        new(false, 500, "Unexpected server error.");
}
