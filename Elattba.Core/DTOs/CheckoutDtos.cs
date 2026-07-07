using ElAtaba.Domain.Enums;

namespace Elattba.Core.DTOs;

public record CreateCheckoutDto(
    int BuyerId,
    int? CarrierId,
    string ShippingAddressSnapshot,
    PaymentMethod PaymentMethod,
    IReadOnlyList<CreateCheckoutItemDto> Items);

public record CreateCheckoutItemDto(
    int ProductId,
    int Quantity);

public record CheckoutResultDto(
    string CheckoutReference,
    decimal TotalAmount,
    IReadOnlyList<CheckoutOrderDto> Orders);

public record CheckoutOrderDto(
    int OrderId,
    int StoreId,
    string? StoreName,
    decimal TotalAmount,
    int ItemCount);
