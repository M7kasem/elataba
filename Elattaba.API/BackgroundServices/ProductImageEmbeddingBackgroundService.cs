using System.Text.Json;
using Elattba.Application.ProductImages;
using Elattba.Core.InterFaces;
using Elattba.Core.Services;

namespace Elattaba.API.BackgroundServices;

public sealed class ProductImageEmbeddingBackgroundService : BackgroundService
{
    private readonly IProductImageEmbeddingQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<ProductImageEmbeddingBackgroundService> _logger;

    public ProductImageEmbeddingBackgroundService(
        IProductImageEmbeddingQueue queue,
        IServiceScopeFactory scopeFactory,
        IWebHostEnvironment environment,
        ILogger<ProductImageEmbeddingBackgroundService> logger)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _environment = environment;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var productImageId in _queue.ReadAllAsync(stoppingToken))
        {
            try
            {
                await ProcessImageAsync(productImageId, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to generate embedding for product image {ProductImageId}.", productImageId);
            }
        }
    }

    private async Task ProcessImageAsync(int productImageId, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var embeddingService = scope.ServiceProvider.GetRequiredService<IImageEmbeddingService>();

        var image = await unitOfWork.ProductImages.GetByIdAsync(productImageId);
        if (image == null)
        {
            return;
        }

        var imagePath = GetLocalImagePath(image.ImageUrl);
        if (imagePath == null || !File.Exists(imagePath))
        {
            _logger.LogInformation("Skipping embedding for product image {ProductImageId}; local file was not found.", productImageId);
            return;
        }

        await using var imageStream = File.OpenRead(imagePath);
        var embedding = await embeddingService.GenerateEmbeddingAsync(imageStream, cancellationToken);
        image.EmbeddingVector = JsonSerializer.Serialize(embedding);

        await unitOfWork.ProductImages.UpdateAsync(image);
        await unitOfWork.CompleteAsync();
    }

    private string? GetLocalImagePath(string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl) || Uri.TryCreate(imageUrl, UriKind.Absolute, out _))
        {
            return null;
        }

        var webRootPath = string.IsNullOrWhiteSpace(_environment.WebRootPath)
            ? Path.Combine(_environment.ContentRootPath, "wwwroot")
            : _environment.WebRootPath;

        var relativePath = imageUrl.TrimStart('/', '\\')
            .Replace('/', Path.DirectorySeparatorChar)
            .Replace('\\', Path.DirectorySeparatorChar);

        var fullPath = Path.GetFullPath(Path.Combine(webRootPath, relativePath));
        var uploadsRoot = Path.GetFullPath(Path.Combine(webRootPath, "uploads"));

        return fullPath.StartsWith(uploadsRoot, StringComparison.OrdinalIgnoreCase)
            ? fullPath
            : null;
    }
}
