using CrowdParlay.Social.Domain.DTOs;
using CrowdParlay.Social.Domain.Entities;

namespace CrowdParlay.Social.Domain.Abstractions;

public interface IDiscussionsRepository : ISubjectsRepository
{
    public Task<Discussion> GetByIdAsync(string discussionId, Guid? viewerId);
    public Task<Page<Discussion>> SearchAsync(Guid? authorId, Guid? viewerId, int offset, int count);
    public Task<string> CreateAsync(Guid authorId, string title, string content);
    public Task UpdateAsync(string discussionId, string? title = null, string? content = null);
}