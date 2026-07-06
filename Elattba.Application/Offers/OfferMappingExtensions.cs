using ElAtaba.Domain.Entities;
using Elattba.Core.DTOs;

namespace Elattba.Application.Offers;

internal static class OfferMappingExtensions
{
    public static OfferDto ToOfferDto(this Offer offer) =>
        new(
            offer.OfferId,
            offer.StoreId,
            offer.Store?.StoreName,
            offer.DiscountPercentage,
            offer.StartDate,
            offer.EndDate,
            offer.AppliesToAllProducts,
            offer.CreatedAt,
            offer.OfferProducts.Select(offerProduct => offerProduct.ProductId).ToList());
}
