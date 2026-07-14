using ElAtaba.Domain.Entities;
using Elattba.Application.Offers;
using Elattba.Core.DTOs;

namespace Elattba.Application.Products;

internal static class ProductMappingExtensions
{
    public static ProductDto ToProductDto(this Product product, IEnumerable<Offer> activeOffers)
    {
        var activeOffer = OfferPricingCalculator.FindBestActiveOffer(product, activeOffers);
        var hasActiveOffer = activeOffer != null;
        var currentPrice = hasActiveOffer
            ? OfferPricingCalculator.CalculateDiscountedPrice(product.BasePrice, activeOffer!.DiscountPercentage)
            : product.BasePrice;

        return new ProductDto(
            product.ProductId,
            product.StoreId,
            product.Store?.StoreName,
            product.CategoryId,
            product.Category?.Name,
            product.Name,
            product.Description,
            product.BasePrice,
            hasActiveOffer ? product.BasePrice : null,
            currentPrice,
            activeOffer?.DiscountPercentage,
            hasActiveOffer,
            product.StockQuantity,
            product.CreatedAt,
            product.Images.Select(image => image.ToProductImageDto()).ToList(),
            product.PricingTiers.Select(tier => tier.ToPricingTierDto()).ToList());
    }

    public static BestDealProductDto ToBestDealProductDto(this Product product, Offer activeOffer)
    {
        var currentPrice = OfferPricingCalculator.CalculateDiscountedPrice(
            product.BasePrice,
            activeOffer.DiscountPercentage);

        return new BestDealProductDto(
            product.ProductId,
            product.StoreId,
            product.Store?.StoreName,
            product.CategoryId,
            product.Category?.Name,
            product.Name,
            product.Description,
            product.BasePrice,
            currentPrice,
            activeOffer.DiscountPercentage,
            activeOffer.EndDate,
            product.StockQuantity,
            product.CreatedAt,
            product.Images.Select(image => image.ToProductImageDto()).ToList(),
            product.PricingTiers.Select(tier => tier.ToPricingTierDto()).ToList());
    }

    public static ProductImageDto ToProductImageDto(this ProductImage image) =>
        new(image.ImageId, image.ProductId, image.ImageUrl, image.IsPrimary, image.CreatedAt);

    private static PricingTierDto ToPricingTierDto(this PricingTier tier) =>
        new(tier.TierId, tier.ProductId, tier.MinQuantity, tier.PricePerUnit);
}
