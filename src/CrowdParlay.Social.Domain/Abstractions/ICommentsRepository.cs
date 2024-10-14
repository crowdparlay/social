using CrowdParlay.Social.Domain.DTOs;
using CrowdParlay.Social.Domain.Entities;

namespace CrowdParlay.Social.Domain.Abstractions;

public interface ICommentsRepository
{
    public Task<Comment> GetByIdAsync(Guid commentId, Guid? viewerId);
    public Task<Page<Comment>> SearchAsync(Guid? subjectId, Guid? authorId, Guid? viewerId, int offset, int count);
    public Task<Guid> CreateAsync(Guid authorId, Guid discussionId, string content);
    public Task<Guid> ReplyToCommentAsync(Guid authorId, Guid parentCommentId, string content);
    public Task DeleteAsync(Guid commentId);
}