namespace App.Business.DTOs.Faq;

public class FaqListQueryDto
{
    public string? Search { get; set; }

    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
