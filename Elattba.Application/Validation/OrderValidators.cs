using Elattba.Core.DTOs;
using FluentValidation;

namespace Elattba.Application.Validation;

public sealed class CreateOrderDtoValidator : AbstractValidator<CreateOrderDto>
{
    public CreateOrderDtoValidator()
    {
        RuleFor(order => order.BuyerId).GreaterThan(0);
        RuleFor(order => order.StoreId).GreaterThan(0);
        RuleFor(order => order.CarrierId)
            .GreaterThan(0)
            .When(order => order.CarrierId.HasValue);
        RuleFor(order => order.ShippingAddressSnapshot).NotEmpty();
        RuleFor(order => order.OrderItems).NotEmpty();
        RuleForEach(order => order.OrderItems).SetValidator(new CreateOrderItemDtoValidator());
    }
}

public sealed class UpdateOrderStatusDtoValidator : AbstractValidator<UpdateOrderStatusDto>
{
    public UpdateOrderStatusDtoValidator()
    {
        RuleFor(order => order.CarrierId)
            .GreaterThan(0)
            .When(order => order.CarrierId.HasValue);
        RuleFor(order => order.ShippingCost).GreaterThanOrEqualTo(0);
        RuleFor(order => order.PaymentStatus).IsInEnum();
        RuleFor(order => order.Status).IsInEnum();
    }
}

public sealed class CreateOrderItemDtoValidator : AbstractValidator<CreateOrderItemDto>
{
    public CreateOrderItemDtoValidator()
    {
        RuleFor(item => item.ProductId).GreaterThan(0);
        RuleFor(item => item.Quantity).GreaterThan(0);
    }
}

public sealed class UpdateOrderItemDtoValidator : AbstractValidator<UpdateOrderItemDto>
{
    public UpdateOrderItemDtoValidator()
    {
        RuleFor(item => item.Quantity).GreaterThan(0);
    }
}
