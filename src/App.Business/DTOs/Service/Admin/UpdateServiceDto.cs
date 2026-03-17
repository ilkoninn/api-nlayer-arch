namespace App.Business.DTOs.Service.Admin;

public class UpdateServiceDto : CreateServiceDto
{
    // Flags to delete images
    public bool DeleteCoverImage { get; set; }
    public bool DeleteIcon { get; set; }
}