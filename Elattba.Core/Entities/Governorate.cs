namespace ElAtaba.Domain.Entities;

/// <summary>
/// Fixed lookup list of Egypt's governorates. Seeded once, rarely changes.
/// Lets the platform measure and report actual reach into remote areas.
/// </summary>
public class Governorate
{
    public int GovernorateId { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<ShippingRate> ShippingRates { get; set; } = new List<ShippingRate>();
}
