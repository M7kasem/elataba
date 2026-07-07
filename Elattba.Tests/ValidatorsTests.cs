using ElAtaba.Domain.Enums;
using Elattba.Application.Validation;
using Elattba.Core.DTOs;

namespace Elattba.Tests;

public sealed class ValidatorsTests
{
    [Fact]
    public void CreateOrderValidator_rejects_empty_items_and_bad_ids()
    {
        var validator = new CreateOrderDtoValidator();
        var dto = new CreateOrderDto(0, 0, -1, "", PaymentMethod.Cash, []);

        var result = validator.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "BuyerId");
        Assert.Contains(result.Errors, error => error.PropertyName == "StoreId");
        Assert.Contains(result.Errors, error => error.PropertyName == "OrderItems");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void CreateReviewValidator_rejects_rating_outside_one_to_five(int rating)
    {
        var validator = new CreateReviewDtoValidator();
        var dto = new CreateReviewDto(1, 1, 1, rating, "ok");

        var result = validator.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Rating");
    }

    [Fact]
    public void CreateOfferValidator_requires_product_ids_for_product_specific_offer()
    {
        var validator = new CreateOfferDtoValidator();
        var dto = new CreateOfferDto(1, 10, DateTime.UtcNow, DateTime.UtcNow.AddDays(1), false, []);

        var result = validator.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "ProductIds");
    }

    [Fact]
    public void CreateProductValidator_rejects_non_positive_price()
    {
        var validator = new CreateProductDtoValidator();
        var dto = new CreateProductDto(1, 1, "name", "description", 0, 1, null);

        var result = validator.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "BasePrice");
    }
}
