namespace App.Business.Services.External;

public class ClaimService(IHttpContextAccessor accessor) : IClaimService
{
    public string GetCurrentUserId()
    {
        if (accessor.HttpContext is null)
            return "ByServer";

        var user = accessor?.HttpContext?.User
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        var userId =
            user.FindFirst("uid")?.Value ??
            user.FindFirst("sub")?.Value;

        if (string.IsNullOrWhiteSpace(userId))
            return "ByApplicant";

        return userId;
    }

	public string GetCurrentUserJwtToken()
    {
        if (accessor.HttpContext is null)
            return string.Empty;

        var authHeader = accessor.HttpContext.Request.Headers["Authorization"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer "))
            return string.Empty;

        return authHeader.Substring("Bearer ".Length).Trim();
	}
}


