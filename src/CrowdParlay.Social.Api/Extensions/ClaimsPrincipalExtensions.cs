using System.Security.Claims;
using CrowdParlay.Social.Application.Exceptions;

namespace CrowdParlay.Social.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid? GetUserId(this ClaimsPrincipal principal)
    {
        var userIdClaim = principal.Claims.FirstOrDefault(claim => claim.Type
            is AuthenticationConstants.CookieAuthenticationUserIdClaim
            or AuthenticationConstants.JwtBearerAuthenticationUserIdClaim);

        return Guid.TryParse(userIdClaim?.Value, out var value) ? value : null;
    }
    
    public static Guid GetRequiredUserId(this ClaimsPrincipal principal) =>
        principal.GetUserId() ?? throw new ForbiddenException();
}