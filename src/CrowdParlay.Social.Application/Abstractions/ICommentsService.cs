using CrowdParlay.Social.Application.DTOs;
using CrowdParlay.Social.Domain.DTOs;

namespace CrowdParlay.Social.Application.Abstractions;

public interface ICommentsService : ISubjectsService
{
    public Task<CommentResponse> GetByIdAsync(string commentId, Guid? viewerId);
    public Task<Page<CommentResponse>> GetRepliesAsync(string subjectId, bool flatten, Guid? viewerId, int offset, int count);
    public Task<CommentResponse> ReplyToDiscussionAsync(string discussionId, Guid authorId, string content);
    public Task<CommentResponse> ReplyToCommentAsync(string commentId, Guid authorId, string content);
    public Task DeleteAsync(string commentId);
}