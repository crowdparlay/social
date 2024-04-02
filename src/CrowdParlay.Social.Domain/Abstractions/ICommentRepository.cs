using CrowdParlay.Social.Domain.DTOs;
using CrowdParlay.Social.Domain.Entities;

namespace CrowdParlay.Social.Domain.Abstractions;

public interface ICommentRepository
{
    public Task<Comment> GetByIdAsync(Guid id);
    public Task<Page<Comment>> SearchAsync(Guid? discussionId, Guid? authorId, int offset, int count);
    public Task<Comment> CreateAsync(Guid authorId, Guid discussionId, string content);
    public Task<Page<Comment>> GetRepliesToCommentAsync(Guid parentCommentId, int offset, int count);
    public Task<Comment> ReplyToCommentAsync(Guid authorId, Guid parentCommentId, string content);
    public Task DeleteAsync(Guid id);
}