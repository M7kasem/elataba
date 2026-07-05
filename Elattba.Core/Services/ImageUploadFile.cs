namespace Elattba.Core.Services;

public sealed record ImageUploadFile(
    Stream Content,
    string FileName,
    string ContentType,
    long Length);
