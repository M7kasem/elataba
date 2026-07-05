using ElAtaba.Domain.Entities;
using Elattba.Core.DTOs;

namespace Elattaba.API.Helper;

public static class DtoMapper
{
    public static CategoryDto ToDto(this Category category) =>
        new(category.CategoryId, category.Name, category.Description);

    public static CarrierDto ToDto(this Carrier carrier) =>
        new(carrier.CarrierId, carrier.Name, carrier.IsActive);

    public static GovernorateDto ToDto(this Governorate governorate) =>
        new(governorate.GovernorateId, governorate.Name);

    public static UserDto ToDto(this User user) =>
        new(
            user.UserId,
            user.Email,
            user.Phone,
            user.Role,
            user.GovernorateId,
            user.Governorate?.Name,
            user.City,
            user.ShippingAddress,
            user.CreatedAt);

    public static StoreDto ToDto(this Store store) =>
        new(
            store.StoreId,
            store.OwnerId,
            store.Owner?.Email,
            store.ManagerId,
            store.Manager?.Email,
            store.CategoryId,
            store.Category?.Name,
            store.StoreName,
            store.Location,
            store.Description,
            store.Rating,
            store.CreatedAt);

    public static ProductDto ToDto(this Product product) =>
        product.ToDto([]);

    public static ProductDto ToDto(this Product product, IEnumerable<Offer> activeOffers)
    {
        var activeOffer = activeOffers
            .Where(offer => AppliesToProduct(offer, product))
            .OrderByDescending(offer => offer.DiscountPercentage)
            .ThenByDescending(offer => offer.CreatedAt)
            .FirstOrDefault();

        var hasActiveOffer = activeOffer != null;
        var currentPrice = hasActiveOffer
            ? CalculateDiscountedPrice(product.BasePrice, activeOffer!.DiscountPercentage)
            : product.BasePrice;

        return new(
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
            product.Images.Select(image => image.ToDto()).ToList(),
            product.PricingTiers.Select(tier => tier.ToDto()).ToList());
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

    private static decimal CalculateDiscountedPrice(decimal basePrice, decimal discountPercentage)
    {
        var discountAmount = basePrice * discountPercentage / 100;
        return decimal.Round(basePrice - discountAmount, 2, MidpointRounding.AwayFromZero);
    }

    public static ProductImageDto ToDto(this ProductImage image) =>
        new(image.ImageId, image.ProductId, image.ImageUrl, image.IsPrimary, image.CreatedAt);

    public static PricingTierDto ToDto(this PricingTier tier) =>
        new(tier.TierId, tier.ProductId, tier.MinQuantity, tier.PricePerUnit);

    public static ShippingRateDto ToDto(this ShippingRate rate) =>
        new(
            rate.ShippingRateId,
            rate.CarrierId,
            rate.Carrier?.Name,
            rate.GovernorateId,
            rate.Governorate?.Name,
            rate.Cost);

    public static OfferDto ToDto(this Offer offer) =>
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

    public static OrderDto ToDto(this Order order) =>
        new(
            order.OrderId,
            order.BuyerId,
            order.Buyer?.Email,
            order.StoreId,
            order.Store?.StoreName,
            order.CarrierId,
            order.Carrier?.Name,
            order.OrderDate,
            order.TotalAmount,
            order.ShippingCost,
            order.ShippingAddressSnapshot,
            order.TrackingNumber,
            order.PaymentMethod,
            order.PaymentStatus,
            order.Status,
            order.CreatedAt,
            order.OrderItems.Select(item => item.ToDto()).ToList());

    public static OrderItemDto ToDto(this OrderItem item) =>
        new(
            item.OrderItemId,
            item.OrderId,
            item.ProductId,
            item.Product?.Name,
            item.Quantity,
            item.UnitPrice,
            item.Subtotal);

    public static ReviewDto ToDto(this Review review) =>
        new(
            review.ReviewId,
            review.OrderId,
            review.StoreId,
            review.Store?.StoreName,
            review.BuyerId,
            review.Buyer?.Email,
            review.Rating,
            review.Comment,
            review.CreatedAt);

    public static MessageDto ToDto(this Message message) =>
        new(
            message.MessageId,
            message.SenderId,
            message.Sender?.Email,
            message.RecipientId,
            message.Recipient?.Email,
            message.ProductId,
            message.Product?.Name,
            message.MessageText,
            message.SentAt,
            message.IsRead);

    public static OfferProductDto ToDto(this OfferProduct offerProduct) =>
        new(offerProduct.OfferId, offerProduct.ProductId);
}
