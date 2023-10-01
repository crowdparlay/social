using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;

namespace CrowdParlay.Social.Api.Routing;

/// <summary>
/// A route attribute responsible for applying a default template containing prefix to the given controller or endpoint route.
/// The default pattern is <c>/api/v{version}/{route}</c>.
/// </summary>
public class ApiRouteAttribute : RouteAttribute
{
    private const string Prefix = "api/v{version:apiVersion}";

    public ApiRouteAttribute(
        [StringSyntax("Route")] string template)
        : base(template.StartsWith("~/")
            ? $"~/{Prefix}/{template[2..]}"
            : $"{Prefix}/{template}") { }
}