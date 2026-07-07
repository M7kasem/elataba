using Elattba.Core.DTOs;
using FluentValidation;

namespace Elattba.Application.Validation;

public sealed class CreateReviewDtoValidator : AbstractValidator<CreateReviewDto>
{
    public CreateReviewDtoValidator()
    {
        RuleFor(review => review.OrderId).GreaterThan(0);
        RuleFor(review => review.StoreId).GreaterThan(0);
        RuleFor(review => review.BuyerId).GreaterThan(0);
        Include(new ReviewRules<CreateReviewDto>(review => review.Rating, review => review.Comment));
    }
}

public sealed class UpdateReviewDtoValidator : AbstractValidator<UpdateReviewDto>
{
    public UpdateReviewDtoValidator()
    {
        Include(new ReviewRules<UpdateReviewDto>(review => review.Rating, review => review.Comment));
    }
}

internal sealed class ReviewRules<T> : AbstractValidator<T>
{
    public ReviewRules(Func<T, int> rating, Func<T, string> comment)
    {
        RuleFor(review => rating(review)).InclusiveBetween(1, 5).WithName("Rating");
        RuleFor(review => comment(review)).NotEmpty().WithName("Comment");
    }
}
