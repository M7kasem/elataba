namespace ElAtaba.Domain.Entities;

/// <summary>
/// Carrier price for a buyer governorate. The order stores ShippingCost as a
/// snapshot because operational rates can change after checkout.
/// </summary>
public class ShippingRate
{
    public int ShippingRateId { get; set; }

    public int CarrierId { get; set; }
    public Carrier? Carrier { get; set; }

    public int GovernorateId { get; set; }
    public Governorate? Governorate { get; set; }

    public decimal Cost { get; set; }
}
