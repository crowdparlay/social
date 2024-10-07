using CrowdParlay.Social.Application.Exceptions;
using CrowdParlay.Social.Domain.Abstractions;
using CrowdParlay.Social.Domain.DTOs;
using CrowdParlay.Social.Domain.Entities;
using Mapster;
using Neo4j.Driver;

namespace CrowdParlay.Social.Infrastructure.Persistence.Services;

public class DiscussionsRepository(IAsyncQueryRunner runner) : IDiscussionsRepository
{
    public async Task<Discussion> GetByIdAsync(Guid id)
    {
        var data = await runner.RunAsync(
            """
            MATCH (discussion:Discussion { Id: $id })-[:AUTHORED_BY]->(author:Author)
            RETURN {
                Id: discussion.Id,
                Title: discussion.Title,
                Description: discussion.Description,
                AuthorId: author.Id,
                CreatedAt: discussion.CreatedAt
            }
            """,
            new { id = id.ToString() });

        if (await data.PeekAsync() is null)
            throw new NotFoundException();

        var record = await data.SingleAsync();
        return record[0].Adapt<Discussion>();
    }

    public async Task<Page<Discussion>> GetAllAsync(int offset, int count)
    {
        var data = await runner.RunAsync(
            """
            MATCH (discussion:Discussion)-[:AUTHORED_BY]->(author:Author)
            WITH discussion, author ORDER BY discussion.CreatedAt DESC
            RETURN {
                TotalCount: COUNT(discussion),
                Items: COLLECT({
                    Id: discussion.Id,
                    Title: discussion.Title,
                    Description: discussion.Description,
                    AuthorId: author.Id,
                    CreatedAt: discussion.CreatedAt
                })[$offset..$offset + $count]
            }
            """,
            new
            {
                offset,
                count
            });

        if (await data.PeekAsync() is null)
        {
            return new Page<Discussion>
            {
                TotalCount = 0,
                Items = Enumerable.Empty<Discussion>()
            };
        }

        var record = await data.SingleAsync();
        return record[0].Adapt<Page<Discussion>>();
    }

    public async Task<Page<Discussion>> GetByAuthorAsync(Guid authorId, int offset, int count)
    {
        var data = await runner.RunAsync(
            """
            MATCH (discussion:Discussion)-[:AUTHORED_BY]->(author:Author { Id: $authorId })
            WITH discussion, author ORDER BY discussion.CreatedAt DESC
            RETURN {
                TotalCount: COUNT(discussion),
                Items: COLLECT({
                    Id: discussion.Id,
                    Title: discussion.Title,
                    Description: discussion.Description,
                    AuthorId: author.Id,
                    CreatedAt: discussion.CreatedAt
                })[$offset..$offset + $count]
            }
            """,
            new
            {
                authorId = authorId.ToString(),
                offset,
                count
            });

        if (await data.PeekAsync() is null)
        {
            return new Page<Discussion>
            {
                TotalCount = 0,
                Items = Enumerable.Empty<Discussion>()
            };
        }

        var record = await data.SingleAsync();
        return record[0].Adapt<Page<Discussion>>();
    }

    public async Task<Guid> CreateAsync(Guid authorId, string title, string description)
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
            RETURN discussion.Id
            """,
            new
            {
                authorId = authorId.ToString(),
                title,
                description
            });

        var record = await data.SingleAsync();
        return record[0].Adapt<Guid>();
    }
}