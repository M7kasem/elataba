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
        return ToActionResult(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _shippingRateService.GetByIdAsync(id);
        return ToActionResult(result);
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

        return ToActionResult(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateShippingRateDto updateShippingRateDto)
    {
        var result = await _shippingRateService.UpdateAsync(id, updateShippingRateDto);
        return ToActionResult(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _shippingRateService.DeleteAsync(id);
        return ToActionResult(result);
    }

    private IActionResult ToActionResult<T>(ServiceResult<T> result)
    {
        var response = new ResponseAPI(result.StatusCode, result.Message, result.Data);

        return result.StatusCode switch
        {
            200 => Ok(response),
            400 => BadRequest(response),
            404 => NotFound(response),
            >= 500 => Problem(statusCode: result.StatusCode, title: "Internal Server Error", detail: result.Message),
            _ => StatusCode(result.StatusCode, response)
        };
    }

    private IActionResult ToActionResult(ServiceResult result)
    {
        var response = new ResponseAPI(result.StatusCode, result.Message);

        return result.StatusCode switch
        {
            200 => Ok(response),
            400 => BadRequest(response),
            404 => NotFound(response),
            >= 500 => Problem(statusCode: result.StatusCode, title: "Internal Server Error", detail: result.Message),
            _ => StatusCode(result.StatusCode, response)
        };
    }
}
