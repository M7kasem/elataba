using Elattaba.API.Helper;
using Elattba.Application.Common;
using Elattba.Application.Offers;
using Elattba.Core.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Elattaba.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OfferController : ControllerBase
{
    private readonly IOfferService _offerService;

    public OfferController(IOfferService offerService)
    {
        _offerService = offerService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _offerService.GetAllAsync();
        return this.ToActionResult(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _offerService.GetByIdAsync(id);
        return this.ToActionResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOfferDto createOfferDto)
    {
        var result = await _offerService.CreateAsync(createOfferDto);
        if (result.Succeeded && result.StatusCode == 201 && result.Data != null)
        {
            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Data.OfferId },
                new ResponseAPI(result.StatusCode, result.Message, result.Data));
        }

        return this.ToActionResult(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateOfferDto updateOfferDto)
    {
        var result = await _offerService.UpdateAsync(id, updateOfferDto);
        return this.ToActionResult(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _offerService.DeleteAsync(id);
        return this.ToActionResult(result);
    }

}
