using ElAtaba.Domain.Entities;
using Elattaba.API.Helper;
using Elattba.Core.DTOs;
using Elattba.Core.InterFaces;
using Microsoft.AspNetCore.Mvc;

namespace Elattaba.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class StoreController : BaseController
{
    public StoreController(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var stores = await _unitOfWork.Stores.GetAllAsync(
                store => store.Owner!,
                store => store.Manager!,
                store => store.Category!);
            var data = stores.Select(store => store.ToDto()).ToList();
            return Ok(new ResponseAPI(200, "Stores retrieved successfully", data));
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseAPI(500, $"An error occurred: {ex.Message}"));
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var stores = await _unitOfWork.Stores.GetAllAsync(
                store => store.Owner!,
                store => store.Manager!,
                store => store.Category!);
            var store = stores.FirstOrDefault(item => item.StoreId == id);
            if (store == null)
            {
                return NotFound(new ResponseAPI(404, "Store not found."));
            }

            return Ok(new ResponseAPI(200, "Store retrieved successfully", store.ToDto()));
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseAPI(500, $"An error occurred: {ex.Message}"));
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStoreDto createStoreDto)
    {
        try
        {
            var owner = await _unitOfWork.Users.GetByIdAsync(createStoreDto.OwnerId);
            if (owner == null)
            {
                return NotFound(new ResponseAPI(404, "Owner not found."));
            }

            User? manager = null;
            if (createStoreDto.ManagerId.HasValue)
            {
                manager = await _unitOfWork.Users.GetByIdAsync(createStoreDto.ManagerId.Value);
                if (manager == null)
                {
                    return NotFound(new ResponseAPI(404, "Manager not found."));
                }
            }

            var category = await _unitOfWork.Categories.GetByIdAsync(createStoreDto.CategoryId);
            if (category == null)
            {
                return NotFound(new ResponseAPI(404, "Category not found."));
            }

            var store = new Store
            {
                OwnerId = createStoreDto.OwnerId,
                ManagerId = createStoreDto.ManagerId,
                CategoryId = createStoreDto.CategoryId,
                StoreName = createStoreDto.StoreName,
                Location = createStoreDto.Location,
                Description = createStoreDto.Description
            };

            await _unitOfWork.Stores.AddAsync(store);
            await _unitOfWork.CompleteAsync();

            store.Owner = owner;
            store.Manager = manager;
            store.Category = category;
            return CreatedAtAction(
                nameof(GetById),
                new { id = store.StoreId },
                new ResponseAPI(201, "Store created successfully", store.ToDto()));
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseAPI(500, $"An error occurred: {ex.Message}"));
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateStoreDto updateStoreDto)
    {
        try
        {
            var store = await _unitOfWork.Stores.GetByIdAsync(id);
            if (store == null)
            {
                return NotFound(new ResponseAPI(404, "Store not found."));
            }

            User? manager = null;
            if (updateStoreDto.ManagerId.HasValue)
            {
                manager = await _unitOfWork.Users.GetByIdAsync(updateStoreDto.ManagerId.Value);
                if (manager == null)
                {
                    return NotFound(new ResponseAPI(404, "Manager not found."));
                }
            }

            var category = await _unitOfWork.Categories.GetByIdAsync(updateStoreDto.CategoryId);
            if (category == null)
            {
                return NotFound(new ResponseAPI(404, "Category not found."));
            }

            store.ManagerId = updateStoreDto.ManagerId;
            store.CategoryId = updateStoreDto.CategoryId;
            store.StoreName = updateStoreDto.StoreName;
            store.Location = updateStoreDto.Location;
            store.Description = updateStoreDto.Description;

            await _unitOfWork.Stores.UpdateAsync(store);
            await _unitOfWork.CompleteAsync();

            store.Manager = manager;
            store.Category = category;
            return Ok(new ResponseAPI(200, "Store updated successfully", store.ToDto()));
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseAPI(500, $"An error occurred: {ex.Message}"));
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var store = await _unitOfWork.Stores.GetByIdAsync(id);
            if (store == null)
            {
                return NotFound(new ResponseAPI(404, "Store not found."));
            }

            await _unitOfWork.Stores.DeleteAsync(id);
            await _unitOfWork.CompleteAsync();

            return Ok(new ResponseAPI(200, "Store deleted successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseAPI(500, $"An error occurred: {ex.Message}"));
        }
    }
}
