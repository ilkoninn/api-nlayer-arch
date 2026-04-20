namespace App.Business.DTOs.Setting.Admin;

public class CreateSettingDto
{
    public string Key { get; set; } = null!;
    public string? Value { get; set; }
    public IFormFile? File { get; set; }
}
