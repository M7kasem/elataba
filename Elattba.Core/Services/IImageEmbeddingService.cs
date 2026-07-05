namespace Elattba.Core.Services;

public interface IImageEmbeddingService
{
    Task<float[]> GenerateEmbeddingAsync(Stream imageStream, CancellationToken cancellationToken = default);
}
