namespace Elattba.Core.DTOs;

public record PricingTierDto(
    int TierId,
    int ProductId,
    int MinQuantity,
    decimal PricePerUnit);

public record CreatePricingTierDto(
    int ProductId,
    int MinQuantity,
    decimal PricePerUnit);

public record UpdatePricingTierDto(
    int MinQuantity,
    decimal PricePerUnit);
