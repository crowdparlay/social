using JWT.Algorithms;
using JWT.Builder;

namespace CrowdParlay.Social.IntegrationTests.Services;

public static class Authorization
{
    public static string ProduceAccessToken(Guid userId) => new JwtBuilder()
        .WithAlgorithm(new HMACSHA256Algorithm())
        .WithSecret(string.Empty)
        .AddClaim("sub", userId)
        .Encode();
}