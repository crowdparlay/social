using CrowdParlay.Social.Domain.DTOs;
using CrowdParlay.Social.Domain.Entities;

namespace CrowdParlay.Social.Domain.Abstractions;

public interface IDiscussionsRepository
{
    public Task<Discussion> GetByIdAsync(Guid discussionId, Guid? viewerId);
    public Task<Page<Discussion>> SearchAsync(Guid? authorId, Guid? viewerId, int offset, int count);
    public Task<Guid> CreateAsync(Guid authorId, string title, string description);
}