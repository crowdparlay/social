namespace CrowdParlay.Social.Domain.Abstractions;

public interface ISubjectsRepository
{
    public Task<ISet<string>> GetReactionsAsync(string subjectId, Guid authorId);
    public Task SetReactionsAsync(string subjectId, Guid authorId, ISet<string> reactions);
    public Task UpdateReactionCountersAsync(string subjectId, IDictionary<string, int> reactionsDiff);
    public Task IncludeCommentInMetadataAsync(string discussionId, Guid authorId);
    public Task ExcludeCommentFromMetadataAsync(string discussionId);

}