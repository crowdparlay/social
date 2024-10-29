using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.IdentityModel.JsonWebTokens;
using StackExchange.Redis;

namespace CrowdParlay.Social.Api.Extensions;

public static class ConfigureAuthenticationExtensions
{
    public static IServiceCollection ConfigureAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var dataProtectionRedisConnectionString = configuration["DATA_PROTECTION_REDIS_CONNECTION_STRING"]!;
        var dataProtectionRedisMultiplexer = ConnectionMultiplexer.Connect(dataProtectionRedisConnectionString);
        services.AddDataProtection().PersistKeysToStackExchangeRedis(dataProtectionRedisMultiplexer);

        var builder = services.AddAuthentication(sharedOptions =>
        {
            sharedOptions.DefaultScheme = "BearerOrCookies";
            sharedOptions.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        });

        builder.AddPolicyScheme("BearerOrCookies", "Bearer or Cookies", options =>
        {
            options.ForwardDefaultSelector = context =>
                context.Request.Headers.ContainsKey("Authorization")
                    ? JwtBearerDefaults.AuthenticationScheme
                    : CookieAuthenticationDefaults.AuthenticationScheme;
        });

        builder.AddJwtBearer(options =>
        {
            options.MapInboundClaims = false;
            options.RequireHttpsMetadata = false;
            options.TokenValidationParameters.ValidateAudience = false;
            options.TokenValidationParameters.ValidateIssuer = false;
            options.TokenValidationParameters.ValidateLifetime = false;
            options.TokenValidationParameters.SignatureValidator = (token, _) => new JsonWebToken(token);
        });

        builder.AddCookie(options =>
        {
            options.Cookie.Name = ".CrowdParlay.Authentication";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            options.ExpireTimeSpan = TimeSpan.FromMinutes(15);
            options.SlidingExpiration = true;
            options.Events.OnRedirectToLogin = _ => Task.CompletedTask;
        });

        return services.AddAuthorization(options =>
        {
            var authorizationPolicy = new AuthorizationPolicyBuilder(
                CookieAuthenticationDefaults.AuthenticationScheme,
                JwtBearerDefaults.AuthenticationScheme);

            authorizationPolicy.RequireAuthenticatedUser();
            authorizationPolicy.RequireAssertion(context => context.User.Identities.Count() == 1);
            options.DefaultPolicy = authorizationPolicy.Build();
        });
    }
}