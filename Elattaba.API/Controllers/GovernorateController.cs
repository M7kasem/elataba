using ElAtaba.Domain.Entities;
using Elattaba.API.Helper;
using Elattba.Core.DTOs;
using Elattba.Core.InterFaces;
using Microsoft.AspNetCore.Mvc;

namespace Elattaba.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GovernorateController : BaseController
{
    public GovernorateController(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var governorates = await _unitOfWork.Governorates.GetAllAsync();
            var data = governorates.Select(governorate => governorate.ToDto()).ToList();
            return Ok(new ResponseAPI(200, "Governorates retrieved successfully", data));
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
            var governorate = await _unitOfWork.Governorates.GetByIdAsync(id);
            if (governorate == null)
            {
                return NotFound(new ResponseAPI(404, "Governorate not found."));
            }

            return Ok(new ResponseAPI(200, "Governorate retrieved successfully", governorate.ToDto()));
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseAPI(500, $"An error occurred: {ex.Message}"));
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateGovernorateDto createGovernorateDto)
    {
        try
        {
            var governorate = new Governorate
            {
                Name = createGovernorateDto.Name
            };

            await _unitOfWork.Governorates.AddAsync(governorate);
            await _unitOfWork.CompleteAsync();

            return CreatedAtAction(
                nameof(GetById),
                new { id = governorate.GovernorateId },
                new ResponseAPI(201, "Governorate created successfully", governorate.ToDto()));
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseAPI(500, $"An error occurred: {ex.Message}"));
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateGovernorateDto updateGovernorateDto)
    {
        try
        {
            var governorate = await _unitOfWork.Governorates.GetByIdAsync(id);
            if (governorate == null)
            {
                return NotFound(new ResponseAPI(404, "Governorate not found."));
            }

            governorate.Name = updateGovernorateDto.Name;

            await _unitOfWork.Governorates.UpdateAsync(governorate);
            await _unitOfWork.CompleteAsync();

            return Ok(new ResponseAPI(200, "Governorate updated successfully", governorate.ToDto()));
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
            var governorate = await _unitOfWork.Governorates.GetByIdAsync(id);
            if (governorate == null)
            {
                return NotFound(new ResponseAPI(404, "Governorate not found."));
            }

            await _unitOfWork.Governorates.DeleteAsync(id);
            await _unitOfWork.CompleteAsync();

            return Ok(new ResponseAPI(200, "Governorate deleted successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseAPI(500, $"An error occurred: {ex.Message}"));
        }
    }
}
