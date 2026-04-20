namespace App.Business.DTOs.Setting;

public class SettingDto : BaseEntityDto
{
    public string Key { get; set; } = null!;
    public string Value { get; set; } = null!;
    public bool IsFile { get; set; }
}
