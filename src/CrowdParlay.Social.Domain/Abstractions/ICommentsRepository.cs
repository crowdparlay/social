using CrowdParlay.Social.Domain.DTOs;
using CrowdParlay.Social.Domain.Entities;

namespace CrowdParlay.Social.Domain.Abstractions;

public interface ICommentsRepository : ISubjectsRepository
{
    public Task<Comment> GetByIdAsync(string commentId, Guid? viewerId);
    public Task<Page<Comment>> GetRepliesAsync(string subjectId, bool flatten, Guid? viewerId, int offset, int count);
    public Task<string> CreateAsync(string? subjectId, Guid authorId, string content);
    public Task<IList<Comment>> GetAncestorsAsync(string commentId, Guid? viewerId);
    public Task IncludeCommentInAncestorsMetadataAsync(IEnumerable<Comment> ancestors, Guid authorId);
    public Task ExcludeCommentFromAncestorsMetadataAsync(IEnumerable<Comment> ancestors);
    public Task DeleteAsync(string commentId);
}