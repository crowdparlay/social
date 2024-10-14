using CrowdParlay.Social.Domain.ValueObjects;
using Mapster;

namespace CrowdParlay.Social.Domain;

public class ReactionMapsterAdapterConfigurator
{
    public static void Configure()
    {
        TypeAdapterConfig<string, Reaction>.NewConfig()
            .MapWith(source => new Reaction(source));

        TypeAdapterConfig<Reaction, string>.NewConfig()
            .MapWith(source => source.Value);
    }
}