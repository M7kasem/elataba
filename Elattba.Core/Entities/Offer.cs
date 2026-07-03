namespace ElAtaba.Domain.Entities;

/// <summary>A store-wide or product-specific promotional offer.</summary>
public class Offer
{
    public int OfferId { get; set; }

    public int StoreId { get; set; }
    public Store? Store { get; set; }

    /// <summary>e.g. 15.00 for 15%.</summary>
    public decimal DiscountPercentage { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    /// <summary>If true, the discount applies to the entire store and OfferProducts is unused.</summary>
    public bool AppliesToAllProducts { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Only populated when AppliesToAllProducts is false.</summary>
    public ICollection<OfferProduct> OfferProducts { get; set; } = new List<OfferProduct>();
}
