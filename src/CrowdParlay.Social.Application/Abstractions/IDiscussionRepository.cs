using CrowdParlay.Social.Application.DTOs;

namespace CrowdParlay.Social.Application.Abstractions;

public interface IDiscussionRepository
{
    public Task<DiscussionDto> GetByIdAsync(Guid id);
    public Task<IEnumerable<DiscussionDto>> GetAllAsync();
    public Task<IEnumerable<DiscussionDto>> GetByAuthorAsync(Guid authorId);
    public Task<DiscussionDto> CreateAsync(Guid authorId, string title, string description);
}