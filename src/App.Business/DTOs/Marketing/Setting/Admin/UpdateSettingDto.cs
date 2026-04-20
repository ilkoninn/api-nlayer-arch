namespace App.Business.DTOs.Setting.Admin;

public class UpdateSettingDto : CreateSettingDto
{
    // Flag to delete file
    public bool DeleteFile { get; set; }
}
