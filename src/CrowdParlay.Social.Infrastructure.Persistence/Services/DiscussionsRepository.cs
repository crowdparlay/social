using CrowdParlay.Social.Application.Exceptions;
using CrowdParlay.Social.Domain.Abstractions;
using CrowdParlay.Social.Domain.Entities;
using Mapster;
using Neo4j.Driver;

namespace CrowdParlay.Social.Infrastructure.Persistence.Services;

public class DiscussionsRepository(IDriver driver) : IDiscussionsRepository
{
    public async Task<Discussion> GetByIdAsync(Guid id)
    {
        await using var session = driver.AsyncSession();
        return await session.ExecuteReadAsync(async runner =>
        {
            var data = await runner.RunAsync(
                """
                MATCH (discussion:Discussion { Id: $id })-[:AUTHORED_BY]->(author:Author)
                RETURN {
                    Id: discussion.Id,
                    Title: discussion.Title,
                    Description: discussion.Description,
                    AuthorId: author.Id
                }
                """,
                new { id = id.ToString() });

            if (await data.PeekAsync() is null)
                throw new NotFoundException();

            var record = await data.SingleAsync();
            return record[0].Adapt<Discussion>();
        });
    }

    public async Task<IEnumerable<Discussion>> GetAllAsync()
    {
        await using var session = driver.AsyncSession();
        return await session.ExecuteReadAsync(async runner =>
        {
            var data = await runner.RunAsync(
                """
                MATCH (discussion:Discussion)-[:AUTHORED_BY]->(author:Author)
                RETURN {
                    Id: discussion.Id,
                    Title: discussion.Title,
                    Description: discussion.Description,
                    AuthorId: author.Id
                }
                """);

            var records = await data.ToListAsync();
            return records.Select(x => x[0]).Adapt<IEnumerable<Discussion>>();
        });
    }

    public async Task<IEnumerable<Discussion>> GetByAuthorAsync(Guid authorId)
    {
        await using var session = driver.AsyncSession();
        return await session.ExecuteReadAsync(async runner =>
        {
            var data = await runner.RunAsync(
                """
                MATCH (discussion:Discussion)-[:AUTHORED_BY]->(author:Author { Id: $authorId })
                RETURN {
                    Id: discussion.Id,
                    Title: discussion.Title,
                    Description: discussion.Description,
                    AuthorId: author.Id
                }
                """, new { authorId = authorId.ToString() });

            var records = await data.ToListAsync();
            return records.Select(x => x[0]).Adapt<IEnumerable<Discussion>>();
        });
    }

    public async Task<Discussion> CreateAsync(Guid authorId, string title, string description)
    {
        await using var session = driver.AsyncSession();
        return await session.ExecuteWriteAsync(async runner =>
        {
            var data = await runner.RunAsync(
                """
                MERGE (author:Author { Id: $authorId })
                CREATE (discussion:Discussion {
                    Id: randomUUID(),
                    Title: $title,
                    Description: $description,
                    CreatedAt: datetime()
                })
                CREATE (discussion)-[:AUTHORED_BY]->(author)
                RETURN {
                    Id: discussion.Id,
                    Title: discussion.Title,
                    Description: discussion.Description,
                    AuthorId: author.Id
                }
                """,
                new
                {
                    authorId = authorId.ToString(),
                    title,
                    description
                });

            var record = await data.SingleAsync();
            return record[0].Adapt<Discussion>();
        });
    }
}