namespace ElAtaba.Domain.Entities;

/// <summary>
/// A buyer's review of a store (store-centric, not per product).
/// Linked to an Order to guarantee the reviewer actually purchased - prevents fake reviews.
/// </summary>
public class Review
{
    public int ReviewId { get; set; }

    public int OrderId { get; set; }
    public Order? Order { get; set; }

    public int StoreId { get; set; }
    public Store? Store { get; set; }

    public int BuyerId { get; set; }
    public User? Buyer { get; set; }

    /// <summary>1 to 5 stars.</summary>
    public int Rating { get; set; }

    public string Comment { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
