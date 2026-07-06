using ElAtaba.Domain.Entities;

namespace Elattba.Application.Offers;

internal static class OfferPricingCalculator
{
    public static Offer? FindBestActiveOffer(Product product, IEnumerable<Offer> activeOffers)
    {
        return activeOffers
            .Where(offer => AppliesToProduct(offer, product))
            .OrderByDescending(offer => offer.DiscountPercentage)
            .ThenByDescending(offer => offer.CreatedAt)
            .FirstOrDefault();
    }

    public static decimal GetCurrentUnitPrice(Product product, IEnumerable<Offer> activeOffers)
    {
        var activeOffer = FindBestActiveOffer(product, activeOffers);

        return activeOffer == null
            ? product.BasePrice
            : CalculateDiscountedPrice(product.BasePrice, activeOffer.DiscountPercentage);
    }

    public static decimal CalculateDiscountedPrice(decimal basePrice, decimal discountPercentage)
    {
        var discountAmount = basePrice * discountPercentage / 100;
        return decimal.Round(basePrice - discountAmount, 2, MidpointRounding.AwayFromZero);
    }

    private static bool AppliesToProduct(Offer offer, Product product)
    {
        if (offer.StoreId != product.StoreId)
        {
            return false;
        }

        return offer.AppliesToAllProducts ||
            offer.OfferProducts.Any(offerProduct => offerProduct.ProductId == product.ProductId);
    }
}
