namespace CrowdParlay.Social.Domain.Abstractions;

public interface IAuthorsRepository
{
    public Task EnsureCreatedAsync(Guid authorId);
}