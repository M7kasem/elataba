using Elattaba.API.Helper;
using Elattba.Application.Auth;
using Elattba.Application.Carriers;
using Elattba.Core.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Elattaba.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CarrierController : ControllerBase
{
    private readonly ICarrierService _carrierService;

    public CarrierController(ICarrierService carrierService)
    {
        _carrierService = carrierService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _carrierService.GetAllAsync();
        return this.ToActionResult(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _carrierService.GetByIdAsync(id);
        return this.ToActionResult(result);
    }

    [HttpPost]
    [Authorize(Policy = AuthConstants.AdminOnlyPolicy)]
    public async Task<IActionResult> Create([FromBody] CreateCarrierDto createCarrierDto)
    {
        var result = await _carrierService.CreateAsync(createCarrierDto);
        if (result.Succeeded && result.StatusCode == 201 && result.Data != null)
        {
            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Data.CarrierId },
                new ResponseAPI(result.StatusCode, result.Message, result.Data));
        }

        return this.ToActionResult(result);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = AuthConstants.AdminOnlyPolicy)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCarrierDto updateCarrierDto)
    {
        var result = await _carrierService.UpdateAsync(id, updateCarrierDto);
        return this.ToActionResult(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = AuthConstants.AdminOnlyPolicy)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _carrierService.DeleteAsync(id);
        return this.ToActionResult(result);
    }
}
