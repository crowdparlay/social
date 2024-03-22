using CrowdParlay.Social.Application.Abstractions;
using CrowdParlay.Social.Application.DTOs;
using CrowdParlay.Social.Application.Exceptions;
using Mapster;
using Neo4j.Driver;

namespace CrowdParlay.Social.Infrastructure.Persistence.Services;

public class AuthorsRepository(IDriver driver) : IAuthorRepository
{
    public async Task<AuthorDto> GetByIdAsync(Guid id)
    {
        await using var session = driver.AsyncSession();
        return await session.ExecuteReadAsync(async runner =>
        {
            var data = await runner.RunAsync(
                """
                MATCH (author:Author { Id: $id })
                RETURN {
                    Id: author.Id,
                    Username: author.Username,
                    DisplayName: author.DisplayName,
                    AvatarUrl: author.AvatarUrl
                }
                """,
                new { id = id.ToString() });

            if (await data.PeekAsync() is null)
                throw new NotFoundException();

            var record = await data.SingleAsync();
            return record[0].Adapt<AuthorDto>();
        });
    }

    public async Task<AuthorDto> CreateAsync(Guid id, string username, string displayName, string? avatarUrl)
    {
        await using var session = driver.AsyncSession();
        return await session.ExecuteWriteAsync(async runner =>
        {
            var data = await runner.RunAsync(
                """
                CREATE (author:Author {
                    Id: $id,
                    Username: $username,
                    DisplayName: $displayName,
                    AvatarUrl: $avatarUrl
                })
                RETURN {
                    Id: author.Id,
                    Username: author.Username,
                    DisplayName: author.DisplayName,
                    AvatarUrl: author.AvatarUrl
                }
                """,
                new
                {
                    id = id.ToString(),
                    username,
                    displayName,
                    avatarUrl
                });

            var record = await data.SingleAsync();
            return record[0].Adapt<AuthorDto>();
        });
    }

    public async Task<AuthorDto> UpdateAsync(Guid id, string username, string displayName, string? avatarUrl)
    {
        await using var session = driver.AsyncSession();
        return await session.ExecuteWriteAsync(async runner =>
        {
            var data = await runner.RunAsync(
                """
                MATCH (author:Author { Id: $id })
                SET author.Username = $username,
                    author.DisplayName = $displayName,
                    author.AvatarUrl = $avatarUrl
                RETURN {
                    Id: author.Id,
                    Username: author.Username,
                    DisplayName: author.DisplayName,
                    AvatarUrl: author.AvatarUrl
                }
                """,
                new
                {
                    id = id.ToString(),
                    username,
                    displayName,
                    avatarUrl
                });

            var record = await data.SingleAsync();
            return record[0].Adapt<AuthorDto>();
        });
    }

    public async Task DeleteAsync(Guid id)
    {
        await using var session = driver.AsyncSession();
        var notFount = await session.ExecuteWriteAsync(async runner =>
        {
            var data = await runner.RunAsync(
                """
                OPTIONAL MATCH (author:Author { Id: $id })
                DETACH DELETE author
                RETURN COUNT(author) = 0
                """,
                new { id = id.ToString() });

            var record = await data.SingleAsync();
            return record[0].As<bool>();
        });

        if (notFount)
            throw new NotFoundException();
    }
}