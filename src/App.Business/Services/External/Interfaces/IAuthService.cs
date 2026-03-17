namespace App.Business.Services.External.Interfaces;

public interface IAuthService
{
	//  ==============================
	//  Public operations
	//  ==============================

	Task<string> GetAccessTokenAsync(LoginDto loginDto);

    //  ==============================
    //  Admin operations
    //  ==============================

    Task<ProfileUserDto> GetMySummaryAsync(CancellationToken ct = default);
    Task<bool> UpdateProfileAsync(UpdateProfileUserDto updateProfileUserDto, CancellationToken ct = default);
}
