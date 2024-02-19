using CrowdParlay.Social.Application.Abstractions;
using CrowdParlay.Social.Application.DTOs;
using Mapster;
using Neo4j.Driver;

namespace CrowdParlay.Social.Infrastructure.Persistence.Services;

public class DiscussionRepository : IDiscussionRepository
{
    private readonly IDriver _driver;

    public DiscussionRepository(IDriver driver) => _driver = driver;

    public async Task<DiscussionDto> GetByIdAsync(Guid id)
    {
        await using var session = _driver.AsyncSession();
        return await session.ExecuteReadAsync(async runner =>
        {
            var data = await runner.RunAsync(
                """
                MATCH (discussion:Discussion { Id: $id })-[:AUTHORED_BY]->(author:Author)
                RETURN {
                    Id: discussion.Id,
                    Title: discussion.Title,
                    Description: discussion.Description,
                    Author: {
                        Id: author.Id,
                        Username: author.Username,
                        DisplayName: author.DisplayName,
                        AvatarUrl: author.AvatarUrl
                    }
                }
                """,
                new { id = id.ToString() });

            var record = await data.SingleAsync();
            return record[0].Adapt<DiscussionDto>();
        });
    }

    public async Task<IEnumerable<DiscussionDto>> GetAllAsync()
    {
        await using var session = _driver.AsyncSession();
        return await session.ExecuteReadAsync(async runner =>
        {
            var data = await runner.RunAsync(
                """
                MATCH (discussion:Discussion)-[:AUTHORED_BY]->(author:Author)
                RETURN {
                    Id: discussion.Id,
                    Title: discussion.Title,
                    Description: discussion.Description,
                    Author: {
                        Id: author.Id,
                        Username: author.Username,
                        DisplayName: author.DisplayName,
                        AvatarUrl: author.AvatarUrl
                    }
                }
                """);

            var record = await data.SingleAsync();
            return record[0].Adapt<IEnumerable<DiscussionDto>>();
        });
    }

    public async Task<IEnumerable<DiscussionDto>> GetByAuthorAsync(Guid authorId)
    {
        await using var session = _driver.AsyncSession();
        return await session.ExecuteReadAsync(async runner =>
        {
            var data = await runner.RunAsync(
                """
                MATCH (discussion:Discussion)-[:AUTHORED_BY]->(author:Author { Id: $authorId })
                RETURN {
                    Id: discussion.Id,
                    Title: discussion.Title,
                    Description: discussion.Description,
                    Author: {
                        Id: author.Id,
                        Username: author.Username,
                        DisplayName: author.DisplayName,
                        AvatarUrl: author.AvatarUrl
                    }
                }
                """, new { authorId = authorId.ToString() });

            var record = await data.SingleAsync();
            return record[0].Adapt<IEnumerable<DiscussionDto>>();
        });
    }

    public async Task<DiscussionDto> CreateAsync(Guid authorId, string title, string description)
    {
        await using var session = _driver.AsyncSession();
        return await session.ExecuteWriteAsync(async runner =>
        {
            var data = await runner.RunAsync(
                """
                MATCH (author:Author { Id: $authorId })
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
                    Author: {
                        Id: author.Id,
                        Username: author.Username,
                        DisplayName: author.DisplayName,
                        AvatarUrl: author.AvatarUrl
                    }
                }
                """,
                new
                {
                    authorId = authorId.ToString(),
                    title,
                    description
                });

            var record = await data.SingleAsync();
            return record[0].Adapt<DiscussionDto>();
        });
    }
}