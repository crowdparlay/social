using CrowdParlay.Social.Application.DTOs;

namespace CrowdParlay.Social.Application.Abstractions;

public interface ICommentRepository
{
    public Task<CommentDto> GetByIdAsync(Guid id);
    public Task<Page<CommentDto>> SearchAsync(Guid? discussionId, Guid? authorId, int offset, int count);
    public Task<CommentDto> CreateAsync(Guid authorId, Guid discussionId, string content);
    public Task<Page<CommentDto>> GetRepliesToCommentAsync(Guid parentCommentId, int offset, int count);
    public Task<CommentDto> ReplyToCommentAsync(Guid authorId, Guid parentCommentId, string content);
    public Task DeleteAsync(Guid id);
}