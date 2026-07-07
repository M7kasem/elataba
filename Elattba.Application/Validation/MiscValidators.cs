using Elattba.Core.DTOs;
using FluentValidation;

namespace Elattba.Application.Validation;

public sealed class CreatePricingTierDtoValidator : AbstractValidator<CreatePricingTierDto>
{
    public CreatePricingTierDtoValidator()
    {
        RuleFor(tier => tier.ProductId).GreaterThan(0);
        RuleFor(tier => tier.MinQuantity).GreaterThan(0);
        RuleFor(tier => tier.PricePerUnit).GreaterThan(0);
    }
}

public sealed class UpdatePricingTierDtoValidator : AbstractValidator<UpdatePricingTierDto>
{
    public UpdatePricingTierDtoValidator()
    {
        RuleFor(tier => tier.MinQuantity).GreaterThan(0);
        RuleFor(tier => tier.PricePerUnit).GreaterThan(0);
    }
}

public sealed class CreateShippingRateDtoValidator : AbstractValidator<CreateShippingRateDto>
{
    public CreateShippingRateDtoValidator()
    {
        RuleFor(rate => rate.CarrierId).GreaterThan(0);
        RuleFor(rate => rate.GovernorateId).GreaterThan(0);
        RuleFor(rate => rate.Cost).GreaterThanOrEqualTo(0);
    }
}

public sealed class UpdateShippingRateDtoValidator : AbstractValidator<UpdateShippingRateDto>
{
    public UpdateShippingRateDtoValidator()
    {
        RuleFor(rate => rate.Cost).GreaterThanOrEqualTo(0);
    }
}

public sealed class CreateStoreDtoValidator : AbstractValidator<CreateStoreDto>
{
    public CreateStoreDtoValidator()
    {
        RuleFor(store => store.OwnerId).GreaterThan(0);
        RuleFor(store => store.ManagerId).GreaterThan(0).When(store => store.ManagerId.HasValue);
        RuleFor(store => store.CategoryId).GreaterThan(0);
        RuleFor(store => store.StoreName).NotEmpty();
        RuleFor(store => store.Location).NotEmpty();
        RuleFor(store => store.Description).NotEmpty();
    }
}

public sealed class UpdateStoreDtoValidator : AbstractValidator<UpdateStoreDto>
{
    public UpdateStoreDtoValidator()
    {
        RuleFor(store => store.ManagerId).GreaterThan(0).When(store => store.ManagerId.HasValue);
        RuleFor(store => store.CategoryId).GreaterThan(0);
        RuleFor(store => store.StoreName).NotEmpty();
        RuleFor(store => store.Location).NotEmpty();
        RuleFor(store => store.Description).NotEmpty();
    }
}

public sealed class CreateMessageDtoValidator : AbstractValidator<CreateMessageDto>
{
    public CreateMessageDtoValidator()
    {
        RuleFor(message => message.SenderId).GreaterThan(0);
        RuleFor(message => message.RecipientId).GreaterThan(0);
        RuleFor(message => message.ProductId).GreaterThan(0).When(message => message.ProductId.HasValue);
        RuleFor(message => message.MessageText).NotEmpty();
    }
}

public sealed class CreateProductImageDtoValidator : AbstractValidator<CreateProductImageDto>
{
    public CreateProductImageDtoValidator()
    {
        RuleFor(image => image.ProductId).GreaterThan(0);
        RuleFor(image => image.ImageUrl).NotEmpty();
    }
}

public sealed class UpdateProductImageDtoValidator : AbstractValidator<UpdateProductImageDto>
{
    public UpdateProductImageDtoValidator()
    {
        RuleFor(image => image.ImageUrl).NotEmpty();
    }
}
