using Elattaba.API.Helper;
using Elattba.Application.PricingTiers;
using Elattba.Core.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Elattaba.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PricingTierController : ControllerBase
{
    private readonly IPricingTierService _pricingTierService;

    public PricingTierController(IPricingTierService pricingTierService)
    {
        _pricingTierService = pricingTierService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _pricingTierService.GetAllAsync();
        return this.ToActionResult(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _pricingTierService.GetByIdAsync(id);
        return this.ToActionResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePricingTierDto createPricingTierDto)
    {
        var result = await _pricingTierService.CreateAsync(createPricingTierDto);
        if (result.Succeeded && result.StatusCode == 201 && result.Data != null)
        {
            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Data.TierId },
                new ResponseAPI(result.StatusCode, result.Message, result.Data));
        }

        return this.ToActionResult(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePricingTierDto updatePricingTierDto)
    {
        var result = await _pricingTierService.UpdateAsync(id, updatePricingTierDto);
        return this.ToActionResult(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _pricingTierService.DeleteAsync(id);
        return this.ToActionResult(result);
    }
}
