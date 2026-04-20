namespace App.Core.Entities;

public class Setting : AuditableEntity
{
    public string Key { get; set; } = null!;
    public string Value { get; set; } = null!;
}
