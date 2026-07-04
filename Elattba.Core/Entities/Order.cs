using ElAtaba.Domain.Enums;

namespace ElAtaba.Domain.Entities;

/// <summary>
/// A buyer's purchase order (header/summary level). Shipping details are
/// snapshotted so later carrier/rate changes do not rewrite purchase history.
/// </summary>
public class Order
{
    public int OrderId { get; set; }

    public int BuyerId { get; set; }
    public User? Buyer { get; set; }

    public int StoreId { get; set; }
    public Store? Store { get; set; }

    public int? CarrierId { get; set; }
    public Carrier? Carrier { get; set; }

    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public decimal TotalAmount { get; set; }
    public decimal ShippingCost { get; set; }

    /// <summary>
    /// Snapshot of the shipping address at the moment of ordering - intentionally
    /// not a live FK, so a later change to the buyer's profile doesn't rewrite history.
    /// </summary>
    public string ShippingAddressSnapshot { get; set; } = string.Empty;

    /// <summary>Optional carrier reference added now so shipment tracking fits the same order record.</summary>
    public string? TrackingNumber { get; set; }

    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    /// <summary>A review can only exist once the order itself exists (purchase-gated reviews).</summary>
    public Review? Review { get; set; }
}
