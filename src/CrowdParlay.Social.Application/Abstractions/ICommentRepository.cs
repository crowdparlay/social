using CrowdParlay.Social.Application.DTOs;

namespace CrowdParlay.Social.Application.Abstractions;

public interface ICommentRepository
{
    public Task<CommentDto> GetByIdAsync(Guid id);
    public Task<IEnumerable<CommentDto>> GetByAuthorAsync(Guid authorId);
    public Task<CommentDto> CreateAsync(Guid authorId, Guid discussionId, string content);
    public Task<CommentDto> ReplyToCommentAsync(Guid authorId, Guid parentCommentId, string content);
    public Task DeleteAsync(Guid id);
}