namespace ElAtaba.Domain.Entities;

/// <summary>
/// A wholesale merchant's storefront on the platform. Store-centric model:
/// each store is independent even if the same person owns several.
/// </summary>
public class Store
{
    public int StoreId { get; set; }

    public int OwnerId { get; set; }
    public User? Owner { get; set; }

    /// <summary>
    /// Optional - the owner may delegate exactly one employee to manage the store.
    /// Kept as a single nullable FK by design (no multi-manager support for the MVP).
    /// </summary>
    public int? ManagerId { get; set; }
    public User? Manager { get; set; }

    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    public string StoreName { get; set; } = string.Empty;

    /// <summary>e.g. "العتبة، شارع الجيش"</summary>
    public string Location { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string? LogoUrl { get; set; }

    /// <summary>Calculated as the average of this store's Reviews.Rating.</summary>
    public decimal Rating { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<Product> Products { get; set; } = new List<Product>();
    public ICollection<Offer> Offers { get; set; } = new List<Offer>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<Category> ProductLines { get; set; } = new List<Category>();
}
