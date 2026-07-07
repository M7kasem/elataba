using ElAtaba.Domain.Entities;
using Elattba.Core.DTOs;

namespace Elattba.Application.Orders;

internal static class OrderMappingExtensions
{
    public static OrderDto ToOrderDto(this Order order) =>
        new(
            order.OrderId,
            order.BuyerId,
            order.Buyer?.Email,
            order.StoreId,
            order.Store?.StoreName,
            order.CarrierId,
            order.Carrier?.Name,
            order.OrderDate,
            order.TotalAmount,
            order.ShippingCost,
            order.ShippingAddressSnapshot,
            order.TrackingNumber,
            order.PaymentMethod,
            order.PaymentStatus,
            order.Status,
            order.CreatedAt,
            order.OrderItems.Select(item => item.ToOrderItemDto()).ToList());

    public static OrderItemDto ToOrderItemDto(this OrderItem item) =>
        new(
            item.OrderItemId,
            item.OrderId,
            item.ProductId,
            item.Product?.Name,
            item.Quantity,
            item.UnitPrice,
            item.Subtotal);
}
