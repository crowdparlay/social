using CrowdParlay.Social.Application.DTOs.Author;

namespace CrowdParlay.Social.Application.Abstractions;

public interface IAuthorRepository
{
    public Task<AuthorDto> FindAsync(Guid id);
    public Task<AuthorDto> CreateAsync(Guid id, string username, string displayName, string? avatarUrl);
    public Task<AuthorDto> UpdateAsync(Guid id, string username, string displayName, string? avatarUrl);
    public Task DeleteAsync(Guid id);
}