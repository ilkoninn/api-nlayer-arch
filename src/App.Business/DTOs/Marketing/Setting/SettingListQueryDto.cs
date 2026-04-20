namespace App.Business.DTOs.Setting;

public class SettingListQueryDto
{
	public string? Search { get; set; }

	public int PageNumber { get; set; } = 1;
	public int PageSize { get; set; } = 20;
}
