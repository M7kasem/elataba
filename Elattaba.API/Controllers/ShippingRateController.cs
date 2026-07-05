using ElAtaba.Domain.Entities;
using Elattaba.API.Helper;
using Elattba.Core.DTOs;
using Elattba.Core.InterFaces;
using Microsoft.AspNetCore.Mvc;

namespace Elattaba.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ShippingRateController : BaseController
{
    public ShippingRateController(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var rates = await _unitOfWork.ShippingRates.GetAllAsync(
                rate => rate.Carrier!,
                rate => rate.Governorate!);
            var data = rates.Select(rate => rate.ToDto()).ToList();
            return Ok(new ResponseAPI(200, "Shipping rates retrieved successfully", data));
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
            var rates = await _unitOfWork.ShippingRates.GetAllAsync(
                rate => rate.Carrier!,
                rate => rate.Governorate!);
            var rate = rates.FirstOrDefault(item => item.ShippingRateId == id);
            if (rate == null)
            {
                return NotFound(new ResponseAPI(404, "Shipping rate not found."));
            }

            return Ok(new ResponseAPI(200, "Shipping rate retrieved successfully", rate.ToDto()));
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseAPI(500, $"An error occurred: {ex.Message}"));
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateShippingRateDto createShippingRateDto)
    {
        try
        {
            var carrier = await _unitOfWork.Carriers.GetByIdAsync(createShippingRateDto.CarrierId);
            if (carrier == null)
            {
                return NotFound(new ResponseAPI(404, "Carrier not found."));
            }

            var governorate = await _unitOfWork.Governorates.GetByIdAsync(createShippingRateDto.GovernorateId);
            if (governorate == null)
            {
                return NotFound(new ResponseAPI(404, "Governorate not found."));
            }

            var rate = new ShippingRate
            {
                CarrierId = createShippingRateDto.CarrierId,
                GovernorateId = createShippingRateDto.GovernorateId,
                Cost = createShippingRateDto.Cost
            };

            await _unitOfWork.ShippingRates.AddAsync(rate);
            await _unitOfWork.CompleteAsync();

            rate.Carrier = carrier;
            rate.Governorate = governorate;
            return CreatedAtAction(
                nameof(GetById),
                new { id = rate.ShippingRateId },
                new ResponseAPI(201, "Shipping rate created successfully", rate.ToDto()));
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseAPI(500, $"An error occurred: {ex.Message}"));
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateShippingRateDto updateShippingRateDto)
    {
        try
        {
            var rate = await _unitOfWork.ShippingRates.GetByIdAsync(id);
            if (rate == null)
            {
                return NotFound(new ResponseAPI(404, "Shipping rate not found."));
            }

            rate.Cost = updateShippingRateDto.Cost;

            await _unitOfWork.ShippingRates.UpdateAsync(rate);
            await _unitOfWork.CompleteAsync();

            return Ok(new ResponseAPI(200, "Shipping rate updated successfully", rate.ToDto()));
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
            var rate = await _unitOfWork.ShippingRates.GetByIdAsync(id);
            if (rate == null)
            {
                return NotFound(new ResponseAPI(404, "Shipping rate not found."));
            }

            await _unitOfWork.ShippingRates.DeleteAsync(id);
            await _unitOfWork.CompleteAsync();

            return Ok(new ResponseAPI(200, "Shipping rate deleted successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseAPI(500, $"An error occurred: {ex.Message}"));
        }
    }
}
