namespace CrowdParlay.Social.Application.Abstractions;

public interface IReactionsService
{
    public Task<ISet<string>> GetAsync(Guid subjectId, Guid viewerId);
    public Task SetAsync(Guid subjectId, Guid viewerId, ISet<string> reactions);
}