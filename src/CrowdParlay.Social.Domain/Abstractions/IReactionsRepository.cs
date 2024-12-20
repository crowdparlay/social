namespace CrowdParlay.Social.Domain.Abstractions;

public interface IReactionsRepository
{
    public Task<ISet<string>> GetAsync(Guid subjectId);
    public Task<ISet<string>> GetAsync(Guid subjectId, Guid viewerId);
    public Task SetAsync(Guid subjectId, Guid viewerId, ISet<string> reactions);
}