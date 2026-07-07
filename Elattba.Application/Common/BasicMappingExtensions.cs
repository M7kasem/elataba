using ElAtaba.Domain.Entities;
using Elattba.Core.DTOs;

namespace Elattba.Application.Common;

public static class BasicMappingExtensions
{
    public static CategoryDto ToDto(this Category category) =>
        new(category.CategoryId, category.Name, category.Description);

    public static CarrierDto ToDto(this Carrier carrier) =>
        new(carrier.CarrierId, carrier.Name, carrier.IsActive);

    public static GovernorateDto ToDto(this Governorate governorate) =>
        new(governorate.GovernorateId, governorate.Name);

    public static PricingTierDto ToDto(this PricingTier tier) =>
        new(tier.TierId, tier.ProductId, tier.MinQuantity, tier.PricePerUnit);
}
