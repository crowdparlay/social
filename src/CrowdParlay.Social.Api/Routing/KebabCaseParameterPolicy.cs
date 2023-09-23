using System.Text.RegularExpressions;

namespace CrowdParlay.Social.Api.Routing;

/// <summary>
/// An endpoint parameter transformer that applies kebab-case naming convention to endpoint routes and parameter names.
/// For example, turns '/Animal/8/AddType' into '/animals/8/add-type'.
/// </summary>
public class KebabCaseParameterPolicy : IOutboundParameterTransformer
{
    public string? TransformOutbound(object? value) => value switch
    {
        // Puts dashes in between lower-to-uppercase letter transitions, then makes the whole string lowercase.
        // If input is null, then null is returned.
        // If input is not null, but its string representation is, then null is returned.
        not null => Regex.Replace(
            value.ToString() ?? string.Empty,
            "([a-z])([A-Z])", "$1-$2").ToLower(),
        _ => null
    };
}