using System.Text.Json.Serialization;
using CrowdParlay.Social.Api;

namespace CrowdParlay.Social.Domain.ValueObjects;

[JsonConverter(typeof(ReactionJsonConverter))]
public class Reaction : IEquatable<Reaction>
{
    public readonly string Value;

    public Reaction(string value)
    {
        if (!AllowedValues.Contains(value))
            throw new ArgumentException("Invalid reaction value.", nameof(value));

        Value = value;
    }

    public static readonly IReadOnlySet<string> AllowedValues = new HashSet<string>
    {
        "\ud83c\udf46", // Eggplant
        "\ud83e\udd74", // Woozy Face
        "\ud83d\udc85", // Nail Polish
        "\u2764\ufe0f", // Red Heart
        "\u2764\ufe0f", // Red Heart
        "\ud83e\udd2e", // Face Vomiting
        "\ud83c\udf7e", // Bottle with Popping Cork
        "\ud83e\udd21", // Clown Face
        "\ud83d\ude0e", // Smiling Face with Sunglasses
        "\ud83d\udd25", // Fire
        "\ud83c\udfc6", // Trophy
        "\ud83c\udf08", // Rainbow
        "\ud83d\ude2d", // Loudly Crying Face
        "\ud83e\udd84", // Unicorn
        "\ud83d\udc4d", // Thumbs Up
        "\ud83d\udc4e", // Thumbs Down
        "спскнчл",
        "чел))"
    };

    public static implicit operator string(Reaction reaction) => reaction.Value;
    public static implicit operator Reaction(string reaction) => new(reaction);

    public override string ToString() => Value;

    public bool Equals(Reaction? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == GetType() && Equals((Reaction)obj);
    }

    public override int GetHashCode() => Value.GetHashCode();
}