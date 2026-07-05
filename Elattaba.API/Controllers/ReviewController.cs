using ElAtaba.Domain.Entities;
using Elattaba.API.Helper;
using Elattba.Core.DTOs;
using Elattba.Core.InterFaces;
using Microsoft.AspNetCore.Mvc;

namespace Elattaba.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ReviewController : BaseController
{
    public ReviewController(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var reviews = await _unitOfWork.Reviews.GetAllAsync(
                review => review.Store!,
                review => review.Buyer!);
            var data = reviews.Select(review => review.ToDto()).ToList();
            return Ok(new ResponseAPI(200, "Reviews retrieved successfully", data));
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
            var reviews = await _unitOfWork.Reviews.GetAllAsync(
                review => review.Store!,
                review => review.Buyer!);
            var review = reviews.FirstOrDefault(item => item.ReviewId == id);
            if (review == null)
            {
                return NotFound(new ResponseAPI(404, "Review not found."));
            }

            return Ok(new ResponseAPI(200, "Review retrieved successfully", review.ToDto()));
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseAPI(500, $"An error occurred: {ex.Message}"));
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReviewDto createReviewDto)
    {
        try
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(createReviewDto.OrderId);
            if (order == null)
            {
                return NotFound(new ResponseAPI(404, "Order not found."));
            }

            var store = await _unitOfWork.Stores.GetByIdAsync(createReviewDto.StoreId);
            if (store == null)
            {
                return NotFound(new ResponseAPI(404, "Store not found."));
            }

            var buyer = await _unitOfWork.Users.GetByIdAsync(createReviewDto.BuyerId);
            if (buyer == null)
            {
                return NotFound(new ResponseAPI(404, "Buyer not found."));
            }

            var review = new Review
            {
                OrderId = createReviewDto.OrderId,
                StoreId = createReviewDto.StoreId,
                BuyerId = createReviewDto.BuyerId,
                Rating = createReviewDto.Rating,
                Comment = createReviewDto.Comment
            };

            await _unitOfWork.Reviews.AddAsync(review);
            await _unitOfWork.CompleteAsync();

            review.Store = store;
            review.Buyer = buyer;
            return CreatedAtAction(
                nameof(GetById),
                new { id = review.ReviewId },
                new ResponseAPI(201, "Review created successfully", review.ToDto()));
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseAPI(500, $"An error occurred: {ex.Message}"));
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateReviewDto updateReviewDto)
    {
        try
        {
            var review = await _unitOfWork.Reviews.GetByIdAsync(id);
            if (review == null)
            {
                return NotFound(new ResponseAPI(404, "Review not found."));
            }

            review.Rating = updateReviewDto.Rating;
            review.Comment = updateReviewDto.Comment;

            await _unitOfWork.Reviews.UpdateAsync(review);
            await _unitOfWork.CompleteAsync();

            return Ok(new ResponseAPI(200, "Review updated successfully", review.ToDto()));
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
            var review = await _unitOfWork.Reviews.GetByIdAsync(id);
            if (review == null)
            {
                return NotFound(new ResponseAPI(404, "Review not found."));
            }

            await _unitOfWork.Reviews.DeleteAsync(id);
            await _unitOfWork.CompleteAsync();

            return Ok(new ResponseAPI(200, "Review deleted successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseAPI(500, $"An error occurred: {ex.Message}"));
        }
    }
}
