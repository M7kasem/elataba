using Elattaba.API.Controllers;
using FluentValidation;

namespace Elattaba.API.Validation;

public sealed class UploadProductImageFormDtoValidator : AbstractValidator<ProductImageController.UploadProductImageFormDto>
{
    public UploadProductImageFormDtoValidator()
    {
        RuleFor(upload => upload.ProductId).GreaterThan(0);
        RuleFor(upload => upload.Image).NotNull();
    }
}

public sealed class UploadManyProductImagesFormDtoValidator : AbstractValidator<ProductImageController.UploadManyProductImagesFormDto>
{
    public UploadManyProductImagesFormDtoValidator()
    {
        RuleFor(upload => upload.ProductId).GreaterThan(0);
        RuleFor(upload => upload.Images).NotEmpty();
        RuleFor(upload => upload.PrimaryImageIndex)
            .Must((upload, index) => index >= 0 && index < upload.Images.Count)
            .When(upload => upload.Images.Count > 0)
            .WithMessage("PrimaryImageIndex is out of range.");
    }
}
