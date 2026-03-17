namespace App.Business.DTOs.Faq.Admin;

public class CreateFaqDto
{
    public string Question { get; set; } = null!; 
    public string Answer { get; set; } = null!;
}
