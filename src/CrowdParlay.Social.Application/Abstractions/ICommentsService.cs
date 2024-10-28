using CrowdParlay.Social.Application.DTOs;
using CrowdParlay.Social.Domain.DTOs;

namespace CrowdParlay.Social.Application.Abstractions;

public interface ICommentsService
{
    public Task<CommentResponse> GetByIdAsync(Guid commentId, Guid? viewerId);
    public Task<Page<CommentResponse>> SearchAsync(Guid? discussionId, Guid? authorId, Guid? viewerId, int offset, int count);
    public Task<CommentResponse> CreateAsync(Guid authorId, Guid discussionId, string content);
    public Task<Page<CommentResponse>> GetRepliesToCommentAsync(Guid parentCommentId, Guid? viewerId, int offset, int count);
    public Task<CommentResponse> ReplyToCommentAsync(Guid authorId, Guid parentCommentId, string content);
    public Task DeleteAsync(Guid id);
}