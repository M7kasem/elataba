namespace ElAtaba.Domain.Entities;

/// <summary>
/// Bulk discount pricing - the core feature of the platform (buy more, pay less per unit).
/// Application logic selects the matching tier based on the order quantity.
/// </summary>
public class PricingTier
{
    public int TierId { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    /// <summary>e.g. 1, 10, 50, 100</summary>
    public int MinQuantity { get; set; }

    /// <summary>Unit price once MinQuantity is reached.</summary>
    public decimal PricePerUnit { get; set; }
}
