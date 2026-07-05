using Elattba.Core.Services;
using Microsoft.AspNetCore.Hosting;

namespace Elattba.InfraStructure.Services;

public sealed class ImageManagementService : IImageManagementService
{
    private const long MaxImageSizeInBytes = 5 * 1024 * 1024;
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    };

    private readonly IWebHostEnvironment _environment;

    public ImageManagementService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<string> AddImageAsync(ImageUploadFile file, string src)
    {
        ValidateImage(file);

        var uploadFolder = GetUploadFolder(src);
        Directory.CreateDirectory(uploadFolder);

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(uploadFolder, fileName);

        await using var fileStream = File.Create(filePath);
        await file.Content.CopyToAsync(fileStream);

        return $"/uploads/{NormalizePathSegment(src)}/{fileName}";
    }

    public async Task<IReadOnlyList<string>> AddImagesAsync(IEnumerable<ImageUploadFile> files, string src)
    {
        var imageUrls = new List<string>();

        try
        {
            foreach (var file in files)
            {
                imageUrls.Add(await AddImageAsync(file, src));
            }
        }
        catch
        {
            foreach (var imageUrl in imageUrls)
            {
                DeleteImage(imageUrl);
            }

            throw;
        }

        return imageUrls;
    }

    public void DeleteImage(string src)
    {
        if (string.IsNullOrWhiteSpace(src) || Uri.TryCreate(src, UriKind.Absolute, out _))
        {
            return;
        }

        var relativePath = src.TrimStart('/', '\\')
            .Replace('/', Path.DirectorySeparatorChar)
            .Replace('\\', Path.DirectorySeparatorChar);

        var webRootPath = GetWebRootPath();
        var filePath = Path.GetFullPath(Path.Combine(webRootPath, relativePath));
        var uploadsRoot = Path.GetFullPath(Path.Combine(webRootPath, "uploads"));

        if (!filePath.StartsWith(uploadsRoot, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    private static void ValidateImage(ImageUploadFile file)
    {
        if (file.Length == 0)
        {
            throw new InvalidOperationException("Image file is required.");
        }

        if (file.Length > MaxImageSizeInBytes)
        {
            throw new InvalidOperationException("Image size must not exceed 5 MB.");
        }

        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException("Only jpg, jpeg, png, and webp images are allowed.");
        }

        if (string.IsNullOrWhiteSpace(file.ContentType) ||
            !file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Uploaded file must be an image.");
        }
    }

    private string GetUploadFolder(string src)
    {
        return Path.Combine(GetWebRootPath(), "uploads", NormalizePathSegment(src));
    }

    private string GetWebRootPath()
    {
        if (!string.IsNullOrWhiteSpace(_environment.WebRootPath))
        {
            return _environment.WebRootPath;
        }

        return Path.Combine(_environment.ContentRootPath, "wwwroot");
    }

    private static string NormalizePathSegment(string src)
    {
        var cleanSegment = src.Trim().Trim('/', '\\');

        if (string.IsNullOrWhiteSpace(cleanSegment) ||
            cleanSegment.Contains("..") ||
            cleanSegment.Contains('/') ||
            cleanSegment.Contains('\\'))
        {
            throw new InvalidOperationException("Invalid image folder name.");
        }

        return cleanSegment;
    }
}
