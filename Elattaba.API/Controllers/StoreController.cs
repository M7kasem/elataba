using Elattaba.API.Helper;
using Elattba.Application.Common;
using Elattba.Application.Stores;
using Elattba.Core.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Elattaba.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class StoreController : ControllerBase
{
    private readonly IStoreService _storeService;

    public StoreController(IStoreService storeService)
    {
        _storeService = storeService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _storeService.GetAllAsync();
        return this.ToActionResult(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _storeService.GetByIdAsync(id);
        return this.ToActionResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStoreDto createStoreDto)
    {
        var result = await _storeService.CreateAsync(createStoreDto);
        if (result.Succeeded && result.StatusCode == 201 && result.Data != null)
        {
            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Data.StoreId },
                new ResponseAPI(result.StatusCode, result.Message, result.Data));
        }

        return this.ToActionResult(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateStoreDto updateStoreDto)
    {
        var result = await _storeService.UpdateAsync(id, updateStoreDto);
        return this.ToActionResult(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _storeService.DeleteAsync(id);
        return this.ToActionResult(result);
    }

}
