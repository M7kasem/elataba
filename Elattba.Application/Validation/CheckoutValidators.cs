using Elattba.Core.DTOs;
using FluentValidation;

namespace Elattba.Application.Validation;

public sealed class CreateCheckoutDtoValidator : AbstractValidator<CreateCheckoutDto>
{
    public CreateCheckoutDtoValidator()
    {
        RuleFor(checkout => checkout.BuyerId).GreaterThan(0);
        RuleFor(checkout => checkout.CarrierId)
            .GreaterThan(0)
            .When(checkout => checkout.CarrierId.HasValue);
        RuleFor(checkout => checkout.ShippingAddressSnapshot).NotEmpty();
        RuleFor(checkout => checkout.PaymentMethod).IsInEnum();
        RuleFor(checkout => checkout.Items).NotEmpty();
        RuleForEach(checkout => checkout.Items).SetValidator(new CreateCheckoutItemDtoValidator());
    }
}

public sealed class CreateCheckoutItemDtoValidator : AbstractValidator<CreateCheckoutItemDto>
{
    public CreateCheckoutItemDtoValidator()
    {
        RuleFor(item => item.ProductId).GreaterThan(0);
        RuleFor(item => item.Quantity).GreaterThan(0);
    }
}
