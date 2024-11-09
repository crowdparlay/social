using CrowdParlay.Social.Domain.Abstractions;
using Neo4j.Driver;

namespace CrowdParlay.Social.Infrastructure.Persistence.Services;

public class AuthorsRepository(IAsyncQueryRunner runner) : IAuthorsRepository
{
    public async Task EnsureCreatedAsync(Guid authorId) =>
        await runner.RunAsync("MERGE (author:Author { Id: $authorId })", new { authorId = authorId.ToString() });
}