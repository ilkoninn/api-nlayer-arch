namespace App.Business.DTOs.Common;

public abstract class AuditableEntityDto : BaseEntityDto
{
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
}
