namespace Elattba.Application.Common;

public record ServiceResult(
    bool Succeeded,
    int StatusCode,
    string Message);

public sealed record ServiceResult<T>(
    bool Succeeded,
    int StatusCode,
    string Message,
    T? Data = default)
    : ServiceResult(Succeeded, StatusCode, Message);
