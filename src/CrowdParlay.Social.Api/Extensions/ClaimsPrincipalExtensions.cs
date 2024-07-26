using System.Security.Claims;

namespace CrowdParlay.Social.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid? GetUserId(this ClaimsPrincipal principal)
    {
        var userIdClaim = principal.Claims.FirstOrDefault(claim => claim.Type
            is AuthenticationConstants.CookieAuthenticationUserIdClaim
            or AuthenticationConstants.BearerAuthenticationUserIdClaim);

        return Guid.TryParse(userIdClaim?.Value, out var value) ? value : null;
    }
}