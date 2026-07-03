namespace ElAtaba.Domain.Entities;

/// <summary>An individual line item ("receipt line") within an order.</summary>
public class OrderItem
{
    public int OrderItemId { get; set; }

    public int OrderId { get; set; }
    public Order? Order { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public int Quantity { get; set; }

    /// <summary>Price at the time of purchase - preserves history if the product's price changes later.</summary>
    public decimal UnitPrice { get; set; }

    /// <summary>Quantity * UnitPrice.</summary>
    public decimal Subtotal { get; set; }
}
