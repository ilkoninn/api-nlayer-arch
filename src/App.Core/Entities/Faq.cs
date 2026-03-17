namespace App.Core.Entities;

public class Faq : AuditableEntity
{
    public string Question { get; set; } = null!;
    public string Answer { get; set; } = null!;
}
