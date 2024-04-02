using CrowdParlay.Social.Domain.Entities;

namespace CrowdParlay.Social.Domain.Abstractions;

public interface IDiscussionsRepository
{
    public Task<Discussion> GetByIdAsync(Guid id);
    public Task<IEnumerable<Discussion>> GetAllAsync();
    public Task<IEnumerable<Discussion>> GetByAuthorAsync(Guid authorId);
    public Task<Discussion> CreateAsync(Guid authorId, string title, string description);
}