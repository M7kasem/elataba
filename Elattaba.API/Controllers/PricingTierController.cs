using ElAtaba.Domain.Entities;
using Elattaba.API.Helper;
using Elattba.Core.DTOs;
using Elattba.Core.InterFaces;
using Microsoft.AspNetCore.Mvc;

namespace Elattaba.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PricingTierController : BaseController
{
    public PricingTierController(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var tiers = await _unitOfWork.PricingTiers.GetAllAsync();
            var data = tiers.Select(tier => tier.ToDto()).ToList();
            return Ok(new ResponseAPI(200, "Pricing tiers retrieved successfully", data));
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
            var tier = await _unitOfWork.PricingTiers.GetByIdAsync(id);
            if (tier == null)
            {
                return NotFound(new ResponseAPI(404, "Pricing tier not found."));
            }

            return Ok(new ResponseAPI(200, "Pricing tier retrieved successfully", tier.ToDto()));
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseAPI(500, $"An error occurred: {ex.Message}"));
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePricingTierDto createPricingTierDto)
    {
        try
        {
            var product = await _unitOfWork.Products.GetByIdAsync(createPricingTierDto.ProductId);
            if (product == null)
            {
                return NotFound(new ResponseAPI(404, "Product not found."));
            }

            var tier = new PricingTier
            {
                ProductId = createPricingTierDto.ProductId,
                MinQuantity = createPricingTierDto.MinQuantity,
                PricePerUnit = createPricingTierDto.PricePerUnit
            };

            await _unitOfWork.PricingTiers.AddAsync(tier);
            await _unitOfWork.CompleteAsync();

            return CreatedAtAction(
                nameof(GetById),
                new { id = tier.TierId },
                new ResponseAPI(201, "Pricing tier created successfully", tier.ToDto()));
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseAPI(500, $"An error occurred: {ex.Message}"));
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePricingTierDto updatePricingTierDto)
    {
        try
        {
            var tier = await _unitOfWork.PricingTiers.GetByIdAsync(id);
            if (tier == null)
            {
                return NotFound(new ResponseAPI(404, "Pricing tier not found."));
            }

            tier.MinQuantity = updatePricingTierDto.MinQuantity;
            tier.PricePerUnit = updatePricingTierDto.PricePerUnit;

            await _unitOfWork.PricingTiers.UpdateAsync(tier);
            await _unitOfWork.CompleteAsync();

            return Ok(new ResponseAPI(200, "Pricing tier updated successfully", tier.ToDto()));
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
            var tier = await _unitOfWork.PricingTiers.GetByIdAsync(id);
            if (tier == null)
            {
                return NotFound(new ResponseAPI(404, "Pricing tier not found."));
            }

            await _unitOfWork.PricingTiers.DeleteAsync(id);
            await _unitOfWork.CompleteAsync();

            return Ok(new ResponseAPI(200, "Pricing tier deleted successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseAPI(500, $"An error occurred: {ex.Message}"));
        }
    }
}
