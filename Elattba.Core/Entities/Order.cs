using ElAtaba.Domain.Enums;

namespace ElAtaba.Domain.Entities;

/// <summary>
/// A buyer's purchase order (header/summary level). One order per store -
/// a cart spanning multiple stores becomes multiple orders.
/// </summary>
public class Order
{
    public int OrderId { get; set; }

    public int BuyerId { get; set; }
    public User? Buyer { get; set; }

    public int StoreId { get; set; }
    public Store? Store { get; set; }

    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Snapshot of the shipping address at the moment of ordering - intentionally
    /// not a live FK, so a later change to the buyer's profile doesn't rewrite history.
    /// </summary>
    public string ShippingAddressSnapshot { get; set; } = string.Empty;

    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    /// <summary>A review can only exist once the order itself exists (purchase-gated reviews).</summary>
    public Review? Review { get; set; }
}
