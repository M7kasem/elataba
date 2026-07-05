using ElAtaba.Domain.Entities;
using Elattaba.API.Helper;
using Elattba.Core.DTOs;
using Elattba.Core.InterFaces;
using Microsoft.AspNetCore.Mvc;

namespace Elattaba.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CarrierController : BaseController
{
    public CarrierController(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var carriers = await _unitOfWork.Carriers.GetAllAsync();
            var data = carriers.Select(carrier => carrier.ToDto()).ToList();
            return Ok(new ResponseAPI(200, "Carriers retrieved successfully", data));
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
            var carrier = await _unitOfWork.Carriers.GetByIdAsync(id);
            if (carrier == null)
            {
                return NotFound(new ResponseAPI(404, "Carrier not found."));
            }

            return Ok(new ResponseAPI(200, "Carrier retrieved successfully", carrier.ToDto()));
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseAPI(500, $"An error occurred: {ex.Message}"));
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCarrierDto createCarrierDto)
    {
        try
        {
            var carrier = new Carrier
            {
                Name = createCarrierDto.Name,
                IsActive = createCarrierDto.IsActive
            };

            await _unitOfWork.Carriers.AddAsync(carrier);
            await _unitOfWork.CompleteAsync();

            return CreatedAtAction(
                nameof(GetById),
                new { id = carrier.CarrierId },
                new ResponseAPI(201, "Carrier created successfully", carrier.ToDto()));
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseAPI(500, $"An error occurred: {ex.Message}"));
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCarrierDto updateCarrierDto)
    {
        try
        {
            var carrier = await _unitOfWork.Carriers.GetByIdAsync(id);
            if (carrier == null)
            {
                return NotFound(new ResponseAPI(404, "Carrier not found."));
            }

            carrier.Name = updateCarrierDto.Name;
            carrier.IsActive = updateCarrierDto.IsActive;

            await _unitOfWork.Carriers.UpdateAsync(carrier);
            await _unitOfWork.CompleteAsync();

            return Ok(new ResponseAPI(200, "Carrier updated successfully", carrier.ToDto()));
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
            var carrier = await _unitOfWork.Carriers.GetByIdAsync(id);
            if (carrier == null)
            {
                return NotFound(new ResponseAPI(404, "Carrier not found."));
            }

            await _unitOfWork.Carriers.DeleteAsync(id);
            await _unitOfWork.CompleteAsync();

            return Ok(new ResponseAPI(200, "Carrier deleted successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseAPI(500, $"An error occurred: {ex.Message}"));
        }
    }
}
