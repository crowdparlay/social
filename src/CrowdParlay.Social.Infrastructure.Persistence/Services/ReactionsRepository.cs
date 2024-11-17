using CrowdParlay.Social.Application.Exceptions;
using CrowdParlay.Social.Domain.Abstractions;
using Neo4j.Driver;

namespace CrowdParlay.Social.Infrastructure.Persistence.Services;

public class ReactionsRepository(IAsyncQueryRunner runner) : IReactionsRepository
{
    public async Task<ISet<string>> GetAsync(Guid subjectId)
    {
        var data = await runner.RunAsync(
            """
            MATCH (:Author)-[reaction:REACTED_TO]->(subject { Id: $subjectId })
            WHERE (subject:Comment OR subject:Discussion)
            RETURN DISTINCT reaction.Value
            """,
            new { subjectId = subjectId.ToString() });

        return await data
            .Select(record => record[0].As<string>())
            .ToHashSetAsync();
    }

    public async Task<ISet<string>> GetAsync(Guid subjectId, Guid viewerId)
    {
        var data = await runner.RunAsync(
            """
            MATCH (:Author { Id: $viewerId })-[reaction:REACTED_TO]->(subject { Id: $subjectId })
            WHERE (subject:Comment OR subject:Discussion)
            RETURN reaction.Value
            """,
            new
            {
                subjectId = subjectId.ToString(),
                viewerId = viewerId.ToString()
            });

        return await data
            .Select(record => record[0].As<string>())
            .ToHashSetAsync();
    }

    public async Task SetAsync(Guid subjectId, Guid viewerId, ISet<string> reactions)
    {
        var data = await runner.RunAsync(
            """
            MATCH (viewer:Author { Id: $viewerId }), (subject { Id: $subjectId })
            WHERE subject:Comment OR subject:Discussion
            OPTIONAL MATCH (viewer)-[reaction:REACTED_TO]->(subject)

            DELETE reaction

            WITH DISTINCT viewer, subject
            FOREACH (newReactionValue IN $reactions |
                CREATE (viewer)-[:REACTED_TO { Value: newReactionValue }]->(subject)
            )

            RETURN COUNT(*)
            """,
            new
            {
                subjectId = subjectId.ToString(),
                viewerId = viewerId.ToString(),
                reactions
            });

        var record = await data.SingleAsync();
        var notFound = record[0].As<int>() == 0;

        if (notFound)
            throw new NotFoundException();
    }
}