using ElAtaba.Domain.Entities;
using Elattba.Application.Common;

namespace Elattba.Application.Offers;

internal static class OfferBusinessRules
{
    public static List<int> GetDistinctProductIds(IEnumerable<int>? productIds) =>
        productIds?.Distinct().ToList() ?? [];

    public static ServiceResult<T>? ValidateOffer<T>(
        decimal discountPercentage,
        DateTime startDate,
        DateTime endDate,
        bool appliesToAllProducts,
        IReadOnlyList<int> productIds)
    {
        var dateAndDiscountError = ValidateDiscountAndDates<T>(discountPercentage, startDate, endDate);
        if (dateAndDiscountError != null)
        {
            return dateAndDiscountError;
        }

        if (productIds.Any(productId => productId <= 0))
        {
            return new ServiceResult<T>(false, 400, "Product ids must be greater than zero.");
        }

        if (appliesToAllProducts && productIds.Count > 0)
        {
            return new ServiceResult<T>(false, 400, "Product ids must be empty when the offer applies to all products.");
        }

        if (!appliesToAllProducts && productIds.Count == 0)
        {
            return new ServiceResult<T>(false, 400, "Product ids are required when the offer does not apply to all products.");
        }

        return null;
    }

    public static ServiceResult<T>? ValidateDiscountAndDates<T>(
        decimal discountPercentage,
        DateTime startDate,
        DateTime endDate)
    {
        if (discountPercentage <= 0 || discountPercentage > 100)
        {
            return new ServiceResult<T>(false, 400, "Discount percentage must be greater than 0 and at most 100.");
        }

        if (startDate >= endDate)
        {
            return new ServiceResult<T>(false, 400, "Offer start date must be before end date.");
        }

        return null;
    }

    public static Offer? FindOverlappingOffer(
        IEnumerable<Offer> existingOffers,
        DateTime startDate,
        DateTime endDate,
        bool appliesToAllProducts,
        IReadOnlyCollection<int> productIds,
        int? excludedOfferId = null)
    {
        var productIdSet = productIds.ToHashSet();

        return existingOffers.FirstOrDefault(offer =>
            offer.OfferId != excludedOfferId &&
            DatesOverlap(offer.StartDate, offer.EndDate, startDate, endDate) &&
            ScopesOverlap(offer, appliesToAllProducts, productIdSet));
    }

    public static string BuildOverlapMessage(Offer overlappingOffer)
    {
        return overlappingOffer.AppliesToAllProducts
            ? $"Offer {overlappingOffer.OfferId} already applies to all products in this store during the same period."
            : $"Offer {overlappingOffer.OfferId} already applies to one or more selected products during the same period.";
    }

    private static bool DatesOverlap(
        DateTime firstStart,
        DateTime firstEnd,
        DateTime secondStart,
        DateTime secondEnd) =>
        firstStart < secondEnd && secondStart < firstEnd;

    private static bool ScopesOverlap(
        Offer existingOffer,
        bool appliesToAllProducts,
        IReadOnlySet<int> productIds)
    {
        if (existingOffer.AppliesToAllProducts || appliesToAllProducts)
        {
            return true;
        }

        return existingOffer.OfferProducts.Any(offerProduct => productIds.Contains(offerProduct.ProductId));
    }
}
