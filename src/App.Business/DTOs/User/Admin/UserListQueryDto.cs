namespace App.Business.DTOs.User.Admin;

public sealed class UserListQueryDto
{
	public string? Search { get; init; }          

	public int PageNumber { get; init; } = 1;
	public int PageSize { get; init; } = 20;      
}
