namespace App.Core.Interfaces;

public interface IClaimService
{
    string GetCurrentUserId();
    string GetCurrentUserJwtToken();
}
