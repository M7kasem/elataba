using Elattaba.API.Controllers;
using FluentValidation;

namespace Elattaba.API.Validation;

public sealed class CreateProductFormDtoValidator : AbstractValidator<ProductController.CreateProductFormDto>
{
    public CreateProductFormDtoValidator()
    {
        Include(new ProductFormRules<ProductController.CreateProductFormDto>(
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

public sealed class CreateProductWithOfferFormDtoValidator : AbstractValidator<ProductController.CreateProductWithOfferFormDto>
{
    public CreateProductWithOfferFormDtoValidator()
    {
        Include(new ProductFormRules<ProductController.CreateProductWithOfferFormDto>(
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

public sealed class UpdateProductFormDtoValidator : AbstractValidator<ProductController.UpdateProductFormDto>
{
    public UpdateProductFormDtoValidator()
    {
        Include(new ProductFormRules<ProductController.UpdateProductFormDto>(
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

internal sealed class ProductFormRules<T> : AbstractValidator<T>
{
    public ProductFormRules(
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
