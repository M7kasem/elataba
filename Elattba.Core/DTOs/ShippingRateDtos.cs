namespace Elattba.Core.DTOs;

public record ShippingRateDto(
    int ShippingRateId,
    int CarrierId,
    string? CarrierName,
    int GovernorateId,
    string? GovernorateName,
    decimal Cost);

public record CreateShippingRateDto(
    int CarrierId,
    int GovernorateId,
    decimal Cost);

public record UpdateShippingRateDto(
    decimal Cost);
