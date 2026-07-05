using ElAtaba.Domain.Entities;
using Elattaba.API.Helper;
using Elattba.Core.DTOs;
using Elattba.Core.InterFaces;
using Microsoft.AspNetCore.Mvc;

namespace Elattaba.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : BaseController
    {
        public CategoryController(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var categories = await _unitOfWork.Categories.GetAllAsync();
                var data = categories.Select(category => category.ToDto()).ToList();
                return Ok(new ResponseAPI(200, "Categories retrieved successfully", data));
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
                var category = await _unitOfWork.Categories.GetByIdAsync(id);
                if (category == null)
                {
                    return NotFound(new ResponseAPI(404, "Category not found."));
                }

                return Ok(new ResponseAPI(200, "Category retrieved successfully", category.ToDto()));
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseAPI(500, $"An error occurred: {ex.Message}"));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCategoryDto createCategoryDto)
        {
            try
            {
                var category = new Category
                {
                    Name = createCategoryDto.Name,
                    Description = createCategoryDto.Description
                };

                await _unitOfWork.Categories.AddAsync(category);
                await _unitOfWork.CompleteAsync();

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = category.CategoryId },
                    new ResponseAPI(201, "Category created successfully", category.ToDto()));
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseAPI(500, $"An error occurred: {ex.Message}"));
            }
            
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryDto updateCategoryDto)
        {
            try
            {
                var category = await _unitOfWork.Categories.GetByIdAsync(id);
                if (category == null)
                {
                    return NotFound(new ResponseAPI(404, "Category not found."));
                }

                category.Name = updateCategoryDto.Name;
                category.Description = updateCategoryDto.Description;

                await _unitOfWork.Categories.UpdateAsync(category);
                await _unitOfWork.CompleteAsync();

                return NoContent();
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
                var category = await _unitOfWork.Categories.GetByIdAsync(id);
                if (category == null)
                {
                    return NotFound(new ResponseAPI(404, "Category not found."));
                }

                await _unitOfWork.Categories.DeleteAsync(category.CategoryId);
                await _unitOfWork.CompleteAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseAPI(500, $"An error occurred: {ex.Message}"));
            }
        }
    }
}
