namespace App.API.Controllers.Auth;

[Route("api/auth")]
[ApiController]
public sealed class AuthController(IAuthService service) : ControllerBase
{
    //  ==============================
    //  Public operations
    //  ==============================

    [HttpPost("login")]
    public async Task<IActionResult> GetToken([FromBody] LoginDto loginDto)
    {
        var token = await service.GetAccessTokenAsync(loginDto);
        return Ok(new { AccessToken = token });
    }

    //  ==============================
    //  Admin operations
    //  ==============================

    [HttpPut("profile")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileUserDto updateProfileUserDto, CancellationToken ct)
    {
        var result = await service.UpdateProfileAsync(updateProfileUserDto, ct);

        if (!result)
            return BadRequest("Dəyişiklik uğurla başa çatmadı.");

        return NoContent();
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMySummary(CancellationToken ct)
    {
        var profile = await service.GetMySummaryAsync(ct);
        return Ok(profile);
	}
}
