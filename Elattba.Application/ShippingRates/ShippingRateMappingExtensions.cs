using ElAtaba.Domain.Entities;
using Elattba.Core.DTOs;

namespace Elattba.Application.ShippingRates;

internal static class ShippingRateMappingExtensions
{
    public static ShippingRateDto ToShippingRateDto(this ShippingRate rate) =>
        new(
            rate.ShippingRateId,
            rate.CarrierId,
            rate.Carrier?.Name,
            rate.GovernorateId,
            rate.Governorate?.Name,
            rate.Cost);
}
