namespace Elattba.Core.DTOs;

public record CarrierDto(
    int CarrierId,
    string Name,
    bool IsActive);

public record CreateCarrierDto(
    string Name,
    bool IsActive);

public record UpdateCarrierDto(
    string Name,
    bool IsActive);
