namespace App.Business.Helpers;

public static class FileChecker
{
    public static void ValidateFile(IFormFile file, long maxFileSize, string[] allowedExtensions)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("Fayl boş və ya mövcud deyil.");
        }

        if (file.Length > maxFileSize)
        {
            throw new InvalidOperationException($"Faylın həcmi maksimum icazə verilən {maxFileSize / (1024 * 1024)}MB həcmindən çoxdur.");
        }

        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (string.IsNullOrEmpty(fileExtension) || !allowedExtensions.Contains(fileExtension))
        {
            throw new InvalidOperationException($"'{fileExtension}' fayl tipi icazə verilmir. İcazə verilən tiplər: {string.Join(", ", allowedExtensions)}");
        }

        // Check if file is actually an image by reading its header
        if (!IsValidImage(file, fileExtension))
        {
            throw new InvalidOperationException("Fayl düzgün şəkil deyil.");
        }
    }

    private static bool IsValidImage(IFormFile file, string extension)
    {
        if (extension == ".svg")
            return IsValidSvg(file);

        try
        {
            using var stream = file.OpenReadStream();
            using var image = Image.Load(stream);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsValidSvg(IFormFile file)
    {
        try
        {
            using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream, Encoding.UTF8, true, 1024);
            var content = reader.ReadToEnd();
            return content.Contains("<svg", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }
}
