using CrowdParlay.Social.Application.DTOs;
using CrowdParlay.Social.Domain.DTOs;
using CrowdParlay.Social.Domain.ValueObjects;

namespace CrowdParlay.Social.Application.Abstractions;

public interface IDiscussionsService
{
    public Task<DiscussionDto> GetByIdAsync(Guid discussionId, Guid? viewerId);
    public Task<Page<DiscussionDto>> SearchAsync(Guid? authorId, Guid? viewerId, int offset, int count);
    public Task<DiscussionDto> CreateAsync(Guid authorId, string title, string description);
    public Task<DiscussionDto> AddReactionAsync(Guid authorId, Guid discussionId, Reaction reaction);
    public Task<DiscussionDto> RemoveReactionAsync(Guid authorId, Guid discussionId, Reaction reaction);
}