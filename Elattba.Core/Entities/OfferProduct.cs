namespace ElAtaba.Domain.Entities;

/// <summary>
/// Junction entity for the many-to-many relationship between Offers and Products.
/// Composite primary key (OfferId, ProductId) is configured in AppDbContext.
/// </summary>
public class OfferProduct
{
    public int OfferId { get; set; }
    public Offer? Offer { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }
}
