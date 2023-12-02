using CrowdParlay.Social.Application.DTOs.Comment;

namespace CrowdParlay.Social.Application.Abstractions;

public interface ICommentRepository
{
    public Task<CommentDto> FindAsync(Guid id);
    public Task<IEnumerable<CommentDto>> FindByAuthorAsync(Guid authorId, int page, int size);
    public Task<CommentDto> CreateAsync(Guid authorId, string content);
    public Task<CommentDto> ReplyAsync(Guid authorId, Guid targetCommentId, string content);
    public Task DeleteAsync(Guid id);
}