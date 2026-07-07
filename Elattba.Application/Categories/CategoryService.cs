using ElAtaba.Domain.Entities;
using Elattba.Application.Common;
using Elattba.Core.DTOs;
using Elattba.Core.InterFaces;

namespace Elattba.Application.Categories;

public sealed class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;

    public CategoryService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult<IReadOnlyList<CategoryDto>>> GetAllAsync()
    {
        try
        {
            var categories = await _unitOfWork.Categories.GetAllAsync();
            var data = categories.Select(category => category.ToDto()).ToList();
            return new ServiceResult<IReadOnlyList<CategoryDto>>(true, 200, "Categories retrieved successfully", data);
        }
        catch (Exception ex)
        {
            return Failure<IReadOnlyList<CategoryDto>>(ex);
        }
    }

    public async Task<ServiceResult<CategoryDto>> GetByIdAsync(int id)
    {
        try
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id);
            return category == null
                ? new ServiceResult<CategoryDto>(false, 404, "Category not found.")
                : new ServiceResult<CategoryDto>(true, 200, "Category retrieved successfully", category.ToDto());
        }
        catch (Exception ex)
        {
            return Failure<CategoryDto>(ex);
        }
    }

    public async Task<ServiceResult<CategoryDto>> CreateAsync(CreateCategoryDto dto)
    {
        try
        {
            var category = new Category
            {
                Name = dto.Name,
                Description = dto.Description
            };

            await _unitOfWork.Categories.AddAsync(category);
            await _unitOfWork.CompleteAsync();

            return new ServiceResult<CategoryDto>(true, 201, "Category created successfully", category.ToDto());
        }
        catch (Exception ex)
        {
            return Failure<CategoryDto>(ex);
        }
    }

    public async Task<ServiceResult<CategoryDto>> UpdateAsync(int id, UpdateCategoryDto dto)
    {
        try
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id);
            if (category == null)
            {
                return new ServiceResult<CategoryDto>(false, 404, "Category not found.");
            }

            category.Name = dto.Name;
            category.Description = dto.Description;

            await _unitOfWork.Categories.UpdateAsync(category);
            await _unitOfWork.CompleteAsync();

            return new ServiceResult<CategoryDto>(true, 200, "Category updated successfully", category.ToDto());
        }
        catch (Exception ex)
        {
            return Failure<CategoryDto>(ex);
        }
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        try
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id);
            if (category == null)
            {
                return new ServiceResult(false, 404, "Category not found.");
            }

            await _unitOfWork.Categories.DeleteAsync(id);
            await _unitOfWork.CompleteAsync();

            return new ServiceResult(true, 200, "Category deleted successfully");
        }
        catch (Exception)
        {
            return new ServiceResult(false, 500, "Unexpected server error.");
        }
    }

    private static ServiceResult<T> Failure<T>(Exception ex) =>
        new(false, 500, "Unexpected server error.");
}
