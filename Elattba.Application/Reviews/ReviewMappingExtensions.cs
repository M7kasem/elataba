using ElAtaba.Domain.Entities;
using Elattba.Core.DTOs;

namespace Elattba.Application.Reviews;

internal static class ReviewMappingExtensions
{
    public static ReviewDto ToReviewDto(this Review review) =>
        new(
            review.ReviewId,
            review.OrderId,
            review.StoreId,
            review.Store?.StoreName,
            review.BuyerId,
            review.Buyer?.Email,
            review.Rating,
            review.Comment,
            review.CreatedAt);
}
