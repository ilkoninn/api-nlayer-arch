namespace App.Business.DTOs.Auth;

public class UpdateProfileUserDto
{
    public string FullName { get; set; } = default!;
    public string UserName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;
    public string Password { get; set; } = default!;

    /// <summary>Müəllim üçün; tələbə ödənişi göndərəcəyi kart / hesab (maskalanmış).</summary>
    public string? PayoutCardHint { get; set; }
}
