namespace App.Business.Services.External;

public class FileService(IWebHostEnvironment environment) : IFileService
{
    private readonly string _baseUploadFolder = Path.Combine(environment.WebRootPath, "uploads");
    private const long MaxFileSize = 5 * 1024 * 1024; // 5MB
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg"];

    public async Task<string> UploadAsync(IFormFile file, string folder, CancellationToken ct = default)
    {
        if (file is null)
            throw new ArgumentNullException(nameof(file));

        if (string.IsNullOrWhiteSpace(folder))
            throw new ArgumentException("Folder adı boş ola bilməz.", nameof(folder));

        // Validate file
        FileChecker.ValidateFile(file, MaxFileSize, AllowedExtensions);

        var uploadFolder = Path.Combine(_baseUploadFolder, folder.Trim());

        // Ensure upload directory exists
        if (!Directory.Exists(uploadFolder))
        {
            Directory.CreateDirectory(uploadFolder);
        }

        // Generate unique filename
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
        var filePath = Path.Combine(uploadFolder, uniqueFileName);

        try
        {
            // Save file
            await using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream, ct);

            // Return relative URL path
            return $"https://api.example.az/uploads/{folder.Trim()}/{uniqueFileName}";
        }
        catch (Exception ex)
        {
            // Cleanup if save failed
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            throw new InvalidOperationException("Faylı yükləmə uğursuz oldu.", ex);
        }
    }

    public async Task<IEnumerable<string>> UploadAsync(IEnumerable<IFormFile> files, string folder, CancellationToken ct = default)
    {
        if (files is null)
            throw new ArgumentNullException(nameof(files));

        if (string.IsNullOrWhiteSpace(folder))
            throw new ArgumentException("Folder adı boş ola bilməz.", nameof(folder));

        var uploadedPaths = new List<string>();

        foreach (var file in files)
        {
            var path = await UploadAsync(file, folder, ct);
            uploadedPaths.Add(path);
        }

        return uploadedPaths;
    }

    public async Task<bool> DeleteAsync(string filePath, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return false;

        try
        {
            // Extract filename from URL path
            var fileName = Path.GetFileName(filePath);
            var physicalPath = Path.Combine(_baseUploadFolder, fileName);

            // Try to find file in any subfolder if direct path doesn't work
            if (!File.Exists(physicalPath))
            {
                var allFiles = Directory.EnumerateFiles(_baseUploadFolder, fileName, SearchOption.AllDirectories);
                if (allFiles.Any())
                {
                    physicalPath = allFiles.First();
                }
            }

            if (File.Exists(physicalPath))
            {
                File.Delete(physicalPath);
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> FileExistsAsync(string filePath, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return false;

        try
        {
            var fileName = Path.GetFileName(filePath);
            var physicalPath = Path.Combine(_baseUploadFolder, fileName);

            if (File.Exists(physicalPath))
                return true;

            // Check in subfolders
            var allFiles = Directory.EnumerateFiles(_baseUploadFolder, fileName, SearchOption.AllDirectories);
            return allFiles.Any();
        }
        catch
        {
            return false;
        }
    }
}