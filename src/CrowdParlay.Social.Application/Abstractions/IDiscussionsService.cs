using CrowdParlay.Social.Application.DTOs;
using CrowdParlay.Social.Domain.DTOs;

namespace CrowdParlay.Social.Application.Abstractions;

public interface IDiscussionsService : ISubjectsService
{
    public Task<DiscussionResponse> GetByIdAsync(string discussionId, Guid? viewerId);
    public Task<Page<DiscussionResponse>> SearchAsync(Guid? authorId, Guid? viewerId, int offset, int count);
    public Task<DiscussionResponse> CreateAsync(Guid authorId, string title, string content);
    public Task<DiscussionResponse> UpdateAsync(string discussionId, Guid viewerId, UpdateDiscussionRequest request);
}