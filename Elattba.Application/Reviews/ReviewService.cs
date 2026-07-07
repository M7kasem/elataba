using ElAtaba.Domain.Entities;
using ElAtaba.Domain.Enums;
using Elattba.Application.Auth;
using Elattba.Application.Common;
using Elattba.Core.DTOs;
using Elattba.Core.InterFaces;

namespace Elattba.Application.Reviews;

public sealed class ReviewService : IReviewService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService? _currentUserService;

    public ReviewService(IUnitOfWork unitOfWork, ICurrentUserService? currentUserService = null)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<ServiceResult<IReadOnlyList<ReviewDto>>> GetAllAsync()
    {
        try
        {
            var reviews = await _unitOfWork.Reviews.ListAsync(
                null,
                true,
                review => review.Store!,
                review => review.Buyer!);

            var data = reviews.Select(review => review.ToReviewDto()).ToList();
            return new ServiceResult<IReadOnlyList<ReviewDto>>(true, 200, "Reviews retrieved successfully", data);
        }
        catch (Exception ex)
        {
            return Failure<IReadOnlyList<ReviewDto>>(ex);
        }
    }

    public async Task<ServiceResult<ReviewDto>> GetByIdAsync(int id)
    {
        try
        {
            var review = await GetReviewWithDetailsAsync(id, disableTracking: true);
            if (review == null)
            {
                return new ServiceResult<ReviewDto>(false, 404, "Review not found.");
            }

            return new ServiceResult<ReviewDto>(true, 200, "Review retrieved successfully", review.ToReviewDto());
        }
        catch (Exception ex)
        {
            return Failure<ReviewDto>(ex);
        }
    }

    public async Task<ServiceResult<ReviewDto>> CreateAsync(CreateReviewDto dto)
    {
        try
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(dto.OrderId);
            if (order == null)
            {
                return new ServiceResult<ReviewDto>(false, 404, "Order not found.");
            }

            var authorizationError = EnsureCurrentBuyer(dto.BuyerId);
            if (authorizationError != null)
            {
                return authorizationError;
            }

            if (order.Status != OrderStatus.Delivered)
            {
                return new ServiceResult<ReviewDto>(false, 400, "Reviews can only be created after the order is delivered.");
            }

            if (order.BuyerId != dto.BuyerId)
            {
                return new ServiceResult<ReviewDto>(false, 400, "Review buyer must match the order buyer.");
            }

            if (order.StoreId != dto.StoreId)
            {
                return new ServiceResult<ReviewDto>(false, 400, "Review store must match the order store.");
            }

            var existingReview = await _unitOfWork.Reviews.AnyAsync(review => review.OrderId == dto.OrderId);
            if (existingReview)
            {
                return new ServiceResult<ReviewDto>(false, 400, "A review already exists for this order.");
            }

            var review = new Review
            {
                OrderId = dto.OrderId,
                StoreId = dto.StoreId,
                BuyerId = dto.BuyerId,
                Rating = dto.Rating,
                Comment = dto.Comment
            };

            await _unitOfWork.Reviews.AddAsync(review);
            await _unitOfWork.CompleteAsync();

            var createdReview = await GetReviewWithDetailsAsync(review.ReviewId, disableTracking: true) ?? review;
            return new ServiceResult<ReviewDto>(true, 201, "Review created successfully", createdReview.ToReviewDto());
        }
        catch (Exception ex)
        {
            return Failure<ReviewDto>(ex);
        }
    }

    public async Task<ServiceResult<ReviewDto>> UpdateAsync(int id, UpdateReviewDto dto)
    {
        try
        {
            var review = await _unitOfWork.Reviews.GetByIdAsync(id);
            if (review == null)
            {
                return new ServiceResult<ReviewDto>(false, 404, "Review not found.");
            }

            var authorizationError = EnsureCurrentBuyer(review.BuyerId);
            if (authorizationError != null)
            {
                return authorizationError;
            }

            review.Rating = dto.Rating;
            review.Comment = dto.Comment;

            await _unitOfWork.Reviews.UpdateAsync(review);
            await _unitOfWork.CompleteAsync();

            var updatedReview = await GetReviewWithDetailsAsync(id, disableTracking: true) ?? review;
            return new ServiceResult<ReviewDto>(true, 200, "Review updated successfully", updatedReview.ToReviewDto());
        }
        catch (Exception ex)
        {
            return Failure<ReviewDto>(ex);
        }
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        try
        {
            var review = await _unitOfWork.Reviews.GetByIdAsync(id);
            if (review == null)
            {
                return new ServiceResult(false, 404, "Review not found.");
            }

            var authorizationError = EnsureCurrentBuyer(review.BuyerId);
            if (authorizationError != null)
            {
                return authorizationError;
            }

            await _unitOfWork.Reviews.DeleteAsync(id);
            await _unitOfWork.CompleteAsync();

            return new ServiceResult(true, 200, "Review deleted successfully");
        }
        catch (Exception)
        {
            return new ServiceResult(false, 500, "Unexpected server error.");
        }
    }

    private Task<Review?> GetReviewWithDetailsAsync(int id, bool disableTracking)
    {
        return _unitOfWork.Reviews.GetFirstOrDefaultAsync(
            review => review.ReviewId == id,
            disableTracking,
            review => review.Store!,
            review => review.Buyer!);
    }

    private static ServiceResult<T> Failure<T>(Exception ex) =>
        new(false, 500, "Unexpected server error.");

    private ServiceResult<ReviewDto>? EnsureCurrentBuyer(int buyerId)
    {
        if (_currentUserService?.IsAuthenticated != true || _currentUserService.Role == AuthConstants.AdminRole)
        {
            return null;
        }

        return _currentUserService.UserId == buyerId
            ? null
            : new ServiceResult<ReviewDto>(false, 403, "You are not allowed to manage reviews for another buyer.");
    }
}
