using Elattaba.API.Helper;
using Elattba.Application.Auth;
using Elattba.Application.Categories;
using Elattba.Core.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Elattaba.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _categoryService.GetAllAsync();
            return this.ToActionResult(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _categoryService.GetByIdAsync(id);
            return this.ToActionResult(result);
        }

        [HttpPost]
        [Authorize(Policy = AuthConstants.AdminOnlyPolicy)]
        public async Task<IActionResult> Create([FromBody] CreateCategoryDto createCategoryDto)
        {
            var result = await _categoryService.CreateAsync(createCategoryDto);
            if (result.Succeeded && result.StatusCode == 201 && result.Data != null)
            {
                return CreatedAtAction(
                    nameof(GetById),
                    new { id = result.Data.CategoryId },
                    new ResponseAPI(result.StatusCode, result.Message, result.Data));
            }

            return this.ToActionResult(result);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = AuthConstants.AdminOnlyPolicy)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryDto updateCategoryDto)
        {
            var result = await _categoryService.UpdateAsync(id, updateCategoryDto);
            return this.ToActionResult(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = AuthConstants.AdminOnlyPolicy)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _categoryService.DeleteAsync(id);
            return this.ToActionResult(result);
        }
    }
}
