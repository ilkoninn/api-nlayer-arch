namespace App.Business.DTOs.Faq;

public class FaqDto : BaseEntityDto
{
    public string Question { get; set; } = null!;
    public string Answer { get; set; } = null!;
}
