using Elattba.Core.DTOs;
using FluentValidation;

namespace Elattba.Application.Validation;

public sealed class CreateOfferDtoValidator : AbstractValidator<CreateOfferDto>
{
    public CreateOfferDtoValidator()
    {
        RuleFor(offer => offer.StoreId).GreaterThan(0);
        Include(new OfferRules<CreateOfferDto>(
            offer => offer.DiscountPercentage,
            offer => offer.StartDate,
            offer => offer.EndDate,
            offer => offer.AppliesToAllProducts,
            offer => offer.ProductIds));
    }
}

public sealed class UpdateOfferDtoValidator : AbstractValidator<UpdateOfferDto>
{
    public UpdateOfferDtoValidator()
    {
        Include(new OfferRules<UpdateOfferDto>(
            offer => offer.DiscountPercentage,
            offer => offer.StartDate,
            offer => offer.EndDate,
            offer => offer.AppliesToAllProducts,
            offer => offer.ProductIds));
    }
}

internal sealed class OfferRules<T> : AbstractValidator<T>
{
    public OfferRules(
        Func<T, decimal> discountPercentage,
        Func<T, DateTime> startDate,
        Func<T, DateTime> endDate,
        Func<T, bool> appliesToAllProducts,
        Func<T, IReadOnlyList<int>> productIds)
    {
        RuleFor(offer => discountPercentage(offer))
            .GreaterThan(0)
            .LessThanOrEqualTo(100)
            .WithName("DiscountPercentage");

        RuleFor(offer => endDate(offer))
            .GreaterThan(offer => startDate(offer))
            .WithName("EndDate")
            .WithMessage("EndDate must be after StartDate.");

        RuleFor(offer => productIds(offer))
            .Empty()
            .When(offer => appliesToAllProducts(offer))
            .WithName("ProductIds")
            .WithMessage("ProductIds must be empty when the offer applies to all products.");

        RuleFor(offer => productIds(offer))
            .NotEmpty()
            .When(offer => !appliesToAllProducts(offer))
            .WithName("ProductIds")
            .WithMessage("ProductIds are required when the offer does not apply to all products.");

        RuleForEach(offer => productIds(offer))
            .GreaterThan(0)
            .WithName("ProductIds");
    }
}
