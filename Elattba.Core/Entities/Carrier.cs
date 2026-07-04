namespace ElAtaba.Domain.Entities;

/// <summary>
/// A shipping carrier the platform can assign to orders. Multiple active carriers
/// let checkout choose the best rate without changing the order schema later.
/// </summary>
public class Carrier
{
    public int CarrierId { get; set; }

    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<ShippingRate> ShippingRates { get; set; } = new List<ShippingRate>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
