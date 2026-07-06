using Elattba.Application.Products;
using Elattba.Core.DTOs;
using FluentValidation;

namespace Elattba.Application.Validation;

public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        Include(new ProductRules<CreateProductCommand>(
            product => product.StoreId,
            product => product.CategoryId,
            product => product.Name,
            product => product.Description,
            product => product.BasePrice,
            product => product.StockQuantity));
        RuleFor(product => product.Images).NotEmpty();
        RuleFor(product => product.PrimaryImageIndex)
            .Must((product, index) => index >= 0 && index < product.Images.Count)
            .When(product => product.Images.Count > 0)
            .WithMessage("PrimaryImageIndex is out of range.");
    }
}

public sealed class CreateProductWithOfferCommandValidator : AbstractValidator<CreateProductWithOfferCommand>
{
    public CreateProductWithOfferCommandValidator()
    {
        Include(new ProductRules<CreateProductWithOfferCommand>(
            product => product.StoreId,
            product => product.CategoryId,
            product => product.Name,
            product => product.Description,
            product => product.BasePrice,
            product => product.StockQuantity));
        RuleFor(product => product.Images).NotEmpty();
        RuleFor(product => product.PrimaryImageIndex)
            .Must((product, index) => index >= 0 && index < product.Images.Count)
            .When(product => product.Images.Count > 0)
            .WithMessage("PrimaryImageIndex is out of range.");
        RuleFor(product => product.DiscountPercentage).GreaterThan(0).LessThanOrEqualTo(100);
        RuleFor(product => product.EndDate)
            .GreaterThan(product => product.StartDate)
            .WithMessage("EndDate must be after StartDate.");
    }
}

public sealed class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        Include(new ProductRules<UpdateProductCommand>(
            _ => 1,
            product => product.CategoryId,
            product => product.Name,
            product => product.Description,
            product => product.BasePrice,
            product => product.StockQuantity));
        RuleFor(product => product.PrimaryImageIndex)
            .Must((product, index) => index >= 0 && index < product.Images.Count)
            .When(product => product.Images.Count > 0)
            .WithMessage("PrimaryImageIndex is out of range.");
    }
}

public sealed class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductDtoValidator()
    {
        Include(new ProductRules<CreateProductDto>(
            product => product.StoreId,
            product => product.CategoryId,
            product => product.Name,
            product => product.Description,
            product => product.BasePrice,
            product => product.StockQuantity));
    }
}

public sealed class UpdateProductDtoValidator : AbstractValidator<UpdateProductDto>
{
    public UpdateProductDtoValidator()
    {
        Include(new ProductRules<UpdateProductDto>(
            _ => 1,
            product => product.CategoryId,
            product => product.Name,
            product => product.Description,
            product => product.BasePrice,
            product => product.StockQuantity));
    }
}

internal sealed class ProductRules<T> : AbstractValidator<T>
{
    public ProductRules(
        Func<T, int> storeId,
        Func<T, int> categoryId,
        Func<T, string> name,
        Func<T, string> description,
        Func<T, decimal> basePrice,
        Func<T, int> stockQuantity)
    {
        RuleFor(product => storeId(product)).GreaterThan(0).WithName("StoreId");
        RuleFor(product => categoryId(product)).GreaterThan(0).WithName("CategoryId");
        RuleFor(product => name(product)).NotEmpty().WithName("Name");
        RuleFor(product => description(product)).NotEmpty().WithName("Description");
        RuleFor(product => basePrice(product)).GreaterThan(0).WithName("BasePrice");
        RuleFor(product => stockQuantity(product)).GreaterThanOrEqualTo(0).WithName("StockQuantity");
    }
}
