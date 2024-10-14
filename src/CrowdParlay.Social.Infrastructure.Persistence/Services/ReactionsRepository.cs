using CrowdParlay.Social.Application.Exceptions;
using CrowdParlay.Social.Domain.Abstractions;
using CrowdParlay.Social.Domain.ValueObjects;
using Mapster;
using Neo4j.Driver;

namespace CrowdParlay.Social.Infrastructure.Persistence.Services;

public class ReactionsRepository(IAsyncQueryRunner runner) : IReactionsRepository
{
    public async Task AddAsync(Guid authorId, Guid subjectId, Reaction reaction)
    {
        var data = await runner.RunAsync(
            """
            MATCH (subject { Id: $subjectId })
            WHERE (subject:Comment OR subject:Discussion)
            MERGE (author:Author { Id: $authorId })
            MERGE (author)-[reaction:REACTED_TO { Value: $reaction }]->(subject)
            RETURN reaction IS NULL
            """,
            new
            {
                authorId = authorId.ToString(),
                subjectId = subjectId.ToString(),
                reaction = reaction.ToString()
            });

        var record = await data.SingleAsync();
        var notFound = record[0].As<bool>();
        
        if (notFound)
            throw new NotFoundException();
    }

    public async Task RemoveAsync(Guid authorId, Guid subjectId, Reaction reaction)
    {
        var data = await runner.RunAsync(
            """
            OPTIONAL MATCH (author:Author { Id: $authorId })-[reaction:REACTED_TO { Value: $reaction }]->(subject { Id: $subjectId })
            WHERE (subject:Comment OR subject:Discussion)
            DELETE reaction
            RETURN COUNT(reaction) = 0
            """,
            new
            {
                authorId = authorId.ToString(),
                subjectId = subjectId.ToString(),
                reaction = reaction.ToString()
            });

        var record = await data.SingleAsync();
        var notFound = record[0].As<bool>();

        if (notFound)
            throw new NotFoundException();
    }

    public async Task<ISet<Reaction>> GetAllAsync(Guid authorId, Guid subjectId)
    {
        var data = await runner.RunAsync(
            """
            MATCH (author:Author { Id: $authorId })-[reaction:REACTED_TO]->(subject { Id: $subjectId })
            WHERE (subject:Comment OR subject:Discussion)
            RETURN reaction.Value
            """,
            new
            {
                authorId = authorId.ToString(),
                subjectId = subjectId.ToString()
            });

        return await data
            .Select(record => record[0].As<string>().Adapt<Reaction>())
            .ToHashSetAsync();
    }
}