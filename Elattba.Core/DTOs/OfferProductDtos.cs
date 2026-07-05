namespace Elattba.Core.DTOs;

public record OfferProductDto(
    int OfferId,
    int ProductId);

public record CreateOfferProductDto(
    int OfferId,
    int ProductId);
