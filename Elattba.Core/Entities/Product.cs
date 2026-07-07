namespace ElAtaba.Domain.Entities;

public class Product
{
    public int ProductId { get; set; }

    public int StoreId { get; set; }
    public Store? Store { get; set; }

    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public decimal BasePrice { get; set; }

    public int StockQuantity { get; set; }

    public bool HasOffer { get; set; }

    public byte[] RowVersion { get; set; } = [];

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


    // Navigation
    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public ICollection<PricingTier> PricingTiers { get; set; } = new List<PricingTier>();
    public ICollection<OfferProduct> OfferProducts { get; set; } = new List<OfferProduct>();
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}
