namespace Elattba.Core.DTOs;

public record OrderItemDto(
    int OrderItemId,
    int OrderId,
    int ProductId,
    string? ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal Subtotal);

public record CreateOrderItemDto(
    int ProductId,
    int Quantity);

public record UpdateOrderItemDto(
    int Quantity);
