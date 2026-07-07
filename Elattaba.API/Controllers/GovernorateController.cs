using Elattaba.API.Helper;
using Elattba.Application.Governorates;
using Elattba.Core.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Elattaba.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GovernorateController : ControllerBase
{
    private readonly IGovernorateService _governorateService;

    public GovernorateController(IGovernorateService governorateService)
    {
        _governorateService = governorateService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _governorateService.GetAllAsync();
        return this.ToActionResult(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _governorateService.GetByIdAsync(id);
        return this.ToActionResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateGovernorateDto createGovernorateDto)
    {
        var result = await _governorateService.CreateAsync(createGovernorateDto);
        if (result.Succeeded && result.StatusCode == 201 && result.Data != null)
        {
            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Data.GovernorateId },
                new ResponseAPI(result.StatusCode, result.Message, result.Data));
        }

        return this.ToActionResult(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateGovernorateDto updateGovernorateDto)
    {
        var result = await _governorateService.UpdateAsync(id, updateGovernorateDto);
        return this.ToActionResult(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _governorateService.DeleteAsync(id);
        return this.ToActionResult(result);
    }
}
