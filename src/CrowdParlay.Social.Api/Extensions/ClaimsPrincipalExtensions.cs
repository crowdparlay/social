using System.Security.Claims;

namespace CrowdParlay.Social.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid? GetUserId(this ClaimsPrincipal principal)
    {
        var subject = principal.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
        return subject is not null
            ? Guid.Parse(subject)
            : null;
    }
}