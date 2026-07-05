namespace Elattba.Core.DTOs;

public record GovernorateDto(
    int GovernorateId,
    string Name);

public record CreateGovernorateDto(
    string Name);

public record UpdateGovernorateDto(
    string Name);
