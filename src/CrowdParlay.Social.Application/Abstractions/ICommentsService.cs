using CrowdParlay.Social.Application.DTOs;
using CrowdParlay.Social.Domain.DTOs;
using CrowdParlay.Social.Domain.ValueObjects;

namespace CrowdParlay.Social.Application.Abstractions;

public interface ICommentsService
{
    public Task<CommentDto> GetByIdAsync(Guid commentId, Guid? viewerId);
    public Task<Page<CommentDto>> SearchAsync(Guid? discussionId, Guid? authorId, Guid? viewerId, int offset, int count);
    public Task<CommentDto> CreateAsync(Guid authorId, Guid discussionId, string content);
    public Task<Page<CommentDto>> GetRepliesToCommentAsync(Guid parentCommentId, Guid? viewerId, int offset, int count);
    public Task<CommentDto> ReplyToCommentAsync(Guid authorId, Guid parentCommentId, string content);
    public Task<CommentDto> AddReactionAsync(Guid authorId, Guid commentId, Reaction reaction);
    public Task<CommentDto> RemoveReactionAsync(Guid authorId, Guid commentId, Reaction reaction);
    public Task DeleteAsync(Guid id);
}