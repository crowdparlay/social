using CrowdParlay.Social.Application.DTOs;
using CrowdParlay.Social.Domain.DTOs;

namespace CrowdParlay.Social.Application.Abstractions;

public interface IDiscussionsService
{
    public Task<DiscussionResponse> GetByIdAsync(Guid discussionId, Guid? viewerId);
    public Task<Page<DiscussionResponse>> SearchAsync(Guid? authorId, Guid? viewerId, int offset, int count);
    public Task<DiscussionResponse> CreateAsync(Guid authorId, string title, string description);
    public Task<DiscussionResponse> UpdateAsync(Guid discussionId, Guid viewerId, UpdateDiscussionRequest request);
}