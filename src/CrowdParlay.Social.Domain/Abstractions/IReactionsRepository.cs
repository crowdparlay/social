namespace CrowdParlay.Social.Domain.Abstractions;

public interface IReactionsRepository
{
    public Task AddAsync(Guid authorId, Guid subjectId, string reaction);
    public Task RemoveAsync(Guid authorId, Guid subjectId, string reaction);
    public Task<ISet<string>> GetAllAsync(Guid authorId, Guid subjectId);
}