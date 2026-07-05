namespace Elattba.Core.Services;

public interface IImageManagementService
{
    Task<string> AddImageAsync(ImageUploadFile file, string src);
    Task<IReadOnlyList<string>> AddImagesAsync(IEnumerable<ImageUploadFile> files, string src);
    void DeleteImage(string src);
}
