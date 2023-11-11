using System.Text.Json;
using System.Text.RegularExpressions;

namespace CrowdParlay.Social.Api.Services;

public class SnakeCaseJsonNamingPolicy : JsonNamingPolicy
{
    public override string ConvertName(string name) =>
        Regex.Replace(name, "([a-z0-9])([A-Z])", "$1_$2").ToLower();
}