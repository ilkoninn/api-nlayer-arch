namespace App.Business.Services.External;

public class FileService(IWebHostEnvironment environment) : IFileService
{
    private readonly string _baseUploadFolder = Path.Combine(environment.WebRootPath, "uploads");
    private const long MaxImageFileSize = 5 * 1024 * 1024; // 5MB
    private const long MaxMaterialFileSize = 20 * 1024 * 1024; // 20MB
    private static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg"];
    private static readonly string[] AllowedMaterialExtensions =
    [
        ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg",
        ".pdf",
        ".doc", ".docx",
        ".xls", ".xlsx",
        ".ppt", ".pptx",
        ".txt", ".csv", ".rtf",
        ".zip", ".rar", ".7z"
    ];

    public async Task<string> UploadAsync(IFormFile file, string folder, CancellationToken ct = default)
        => await UploadInternalAsync(
            file,
            folder,
            MaxImageFileSize,
            AllowedImageExtensions,
            ensureImageContent: true,
            ct);

    public async Task<string> UploadMaterialAsync(IFormFile file, string folder, CancellationToken ct = default)
        => await UploadInternalAsync(
            file,
            folder,
            MaxMaterialFileSize,
            AllowedMaterialExtensions,
            ensureImageContent: false,
            ct);

    private async Task<string> UploadInternalAsync(
        IFormFile file,
        string folder,
        long maxFileSize,
        string[] allowedExtensions,
        bool ensureImageContent,
        CancellationToken ct)
    {
        if (file is null)
            throw new ArgumentNullException(nameof(file));

        if (string.IsNullOrWhiteSpace(folder))
            throw new ArgumentException("Folder adı boş ola bilməz.", nameof(folder));

        // Validate file
        FileChecker.ValidateFile(file, maxFileSize, allowedExtensions, ensureImageContent);

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
            return $"https://dev-asiyaflowers.runasp.net/uploads/{folder.Trim()}/{uniqueFileName}";
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

    public Task<bool> DeleteAsync(string filePath, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return Task.FromResult(false);

        try
        {
            var fileName = Path.GetFileName(filePath);
            var physicalPath = Path.Combine(_baseUploadFolder, fileName);

            if (!File.Exists(physicalPath))
            {
                var allFiles = Directory.EnumerateFiles(_baseUploadFolder, fileName, SearchOption.AllDirectories);
                if (allFiles.Any())
                    physicalPath = allFiles.First();
            }

            if (File.Exists(physicalPath))
            {
                File.Delete(physicalPath);
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public Task<bool> FileExistsAsync(string filePath, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return Task.FromResult(false);

        try
        {
            var fileName = Path.GetFileName(filePath);
            var physicalPath = Path.Combine(_baseUploadFolder, fileName);

            if (File.Exists(physicalPath))
                return Task.FromResult(true);

            var allFiles = Directory.EnumerateFiles(_baseUploadFolder, fileName, SearchOption.AllDirectories);
            return Task.FromResult(allFiles.Any());
        }
        catch
        {
            return Task.FromResult(false);
        }
    }
}
