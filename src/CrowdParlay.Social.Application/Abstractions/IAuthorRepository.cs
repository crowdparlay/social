using CrowdParlay.Social.Application.DTOs;

namespace CrowdParlay.Social.Application.Abstractions;

public interface IAuthorRepository
{
    public Task<AuthorDto> GetByIdAsync(Guid id);
    public Task<AuthorDto> CreateAsync(Guid id, string username, string displayName, string? avatarUrl);
    public Task<AuthorDto> UpdateAsync(Guid id, string username, string displayName, string? avatarUrl);
    public Task DeleteAsync(Guid id);
}