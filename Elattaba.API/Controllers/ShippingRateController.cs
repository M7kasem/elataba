using Elattaba.API.Helper;
using Elattba.Application.Common;
using Elattba.Application.ShippingRates;
using Elattba.Core.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Elattaba.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ShippingRateController : ControllerBase
{
    private readonly IShippingRateService _shippingRateService;

    public ShippingRateController(IShippingRateService shippingRateService)
    {
        _shippingRateService = shippingRateService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _shippingRateService.GetAllAsync();
        return this.ToActionResult(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _shippingRateService.GetByIdAsync(id);
        return this.ToActionResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateShippingRateDto createShippingRateDto)
    {
        var result = await _shippingRateService.CreateAsync(createShippingRateDto);
        if (result.Succeeded && result.StatusCode == 201 && result.Data != null)
        {
            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Data.ShippingRateId },
                new ResponseAPI(result.StatusCode, result.Message, result.Data));
        }

        return this.ToActionResult(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateShippingRateDto updateShippingRateDto)
    {
        var result = await _shippingRateService.UpdateAsync(id, updateShippingRateDto);
        return this.ToActionResult(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _shippingRateService.DeleteAsync(id);
        return this.ToActionResult(result);
    }

}
