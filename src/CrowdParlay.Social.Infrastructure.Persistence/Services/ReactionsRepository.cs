using CrowdParlay.Social.Application.Exceptions;
using CrowdParlay.Social.Domain.Abstractions;
using Mapster;
using Neo4j.Driver;
using Neo4j.Driver.Preview.Mapping;

namespace CrowdParlay.Social.Infrastructure.Persistence.Services;

public class ReactionsRepository(IAsyncQueryRunner runner) : IReactionsRepository
{
    public async Task AddAsync(Guid authorId, Guid subjectId, string reaction)
    {
        var data = await runner.RunAsync(
            """
            OPTIONAL MATCH (subject { Id: $subjectId })
            WHERE (subject:Comment OR subject:Discussion)
            MERGE (author:Author { Id: $authorId })-[reaction:REACTED_TO { Reaction: $reaction }]->(subject)
            RETURN COUNT(reaction) = 0
            """,
            new
            {
                authorId = authorId.ToString(),
                subjectId = subjectId.ToString(),
                reaction
            });

        var record = await data.SingleAsync();
        var notFount = record[0].As<bool>();

        if (notFount)
            throw new NotFoundException();
    }

    public async Task RemoveAsync(Guid authorId, Guid subjectId, string reaction)
    {
        var data = await runner.RunAsync(
            """
            OPTIONAL MATCH (author:Author { Id: $authorId })-[reaction:REACTED_TO { Reaction: $reaction }]->(subject { Id: $subjectId })
            WHERE (subject:Comment OR subject:Discussion)
            DELETE reaction
            RETURN COUNT(reaction) = 0
            """,
            new
            {
                authorId = authorId.ToString(),
                subjectId = subjectId.ToString(),
                reaction
            });

        var record = await data.SingleAsync();
        var notFount = record[0].As<bool>();

        if (notFount)
            throw new NotFoundException();
    }

    public async Task<ISet<string>> GetAllAsync(Guid authorId, Guid subjectId)
    {
        var data = await runner.RunAsync(
            """
            MATCH (author:Author)-[reaction:REACTED_TO]->(subject { Id: $subjectId })
            WHERE (subject:Comment OR subject:Discussion)
            RETURN reaction.Reaction
            """,
            new { subjectId = subjectId.ToString() });

        return await data
            .Select(record => record[0].As<string>())
            .ToHashSetAsync();
    }
}