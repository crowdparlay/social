using CrowdParlay.Social.Domain.ValueObjects;

namespace CrowdParlay.Social.Domain.Abstractions;

public interface IReactionsRepository
{
    public Task AddAsync(Guid authorId, Guid subjectId, Reaction reaction);
    public Task RemoveAsync(Guid authorId, Guid subjectId, Reaction reaction);
    public Task<ISet<Reaction>> GetAllAsync(Guid authorId, Guid subjectId);
}