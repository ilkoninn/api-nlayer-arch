namespace App.Core.Entities.Common;

public sealed class PagedResult<T>
{
	public required IReadOnlyList<T> Items { get; init; }
	public required int PageSize { get; init; } = 1;
	public required int PageNumber { get; init; }
	public required int TotalCount { get; init; }

	public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
	public bool HasNext => PageNumber < TotalPages;
	public bool HasPrevious => PageNumber > 1;
}
