namespace ElAtaba.Domain.Entities;

/// <summary>
/// Fixed lookup list of store categories (clothing, accessories, watches...).
/// Replaces a free-text category field so filtering/search stays reliable.
/// </summary>
public class Category
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public ICollection<Store> Stores { get; set; } = new List<Store>();
    public ICollection<Product> Products { get; set; } = new HashSet<Product>();
}
