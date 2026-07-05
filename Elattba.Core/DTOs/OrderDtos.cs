using ElAtaba.Domain.Enums;

namespace Elattba.Core.DTOs;

public record OrderDto(
    int OrderId,
    int BuyerId,
    string? BuyerEmail,
    int StoreId,
    string? StoreName,
    int? CarrierId,
    string? CarrierName,
    DateTime OrderDate,
    decimal TotalAmount,
    decimal ShippingCost,
    string ShippingAddressSnapshot,
    string? TrackingNumber,
    PaymentMethod PaymentMethod,
    PaymentStatus PaymentStatus,
    OrderStatus Status,
    DateTime CreatedAt,
    IReadOnlyList<OrderItemDto> OrderItems);

public record CreateOrderDto(
    int BuyerId,
    int StoreId,
    int? CarrierId,
    string ShippingAddressSnapshot,
    PaymentMethod PaymentMethod,
    IReadOnlyList<CreateOrderItemDto> OrderItems);

public record UpdateOrderStatusDto(
    int? CarrierId,
    decimal ShippingCost,
    string? TrackingNumber,
    PaymentStatus PaymentStatus,
    OrderStatus Status);
