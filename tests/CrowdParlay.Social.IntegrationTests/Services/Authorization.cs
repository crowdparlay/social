using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace CrowdParlay.Social.IntegrationTests.Services;

public static class Authorization
{
    public static string ProduceAccessToken(Guid userId)
    {
        var securityKey = new SymmetricSecurityKey("Secret key of 32 chars for tests"u8.ToArray());
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var subClaim = new Claim(JwtRegisteredClaimNames.Sub, userId.ToString());

        var token = new JwtSecurityToken(
            claims: [subClaim],
            signingCredentials: credentials);

        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.WriteToken(token);
    }
}