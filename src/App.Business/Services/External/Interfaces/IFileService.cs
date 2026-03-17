namespace App.Business.Services.External.Interfaces;

public interface IFileService
{
    Task<string> UploadAsync(IFormFile file, string folder, CancellationToken ct = default);
    Task<IEnumerable<string>> UploadAsync(IEnumerable<IFormFile> files, string folder, CancellationToken ct = default);
    Task<bool> DeleteAsync(string filePath, CancellationToken ct = default);
    Task<bool> FileExistsAsync(string filePath, CancellationToken ct = default);
}