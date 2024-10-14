using System.Text.Json;
using System.Text.Json.Serialization;
using CrowdParlay.Social.Domain.ValueObjects;

namespace CrowdParlay.Social.Api;

public class ReactionJsonConverter : JsonConverter<Reaction>
{
    public override Reaction? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException();

        var value = reader.GetString() ?? throw new JsonException();
        return new Reaction(value);
    }

    public override void Write(Utf8JsonWriter writer, Reaction value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value);
}