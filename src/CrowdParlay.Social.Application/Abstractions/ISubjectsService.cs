namespace CrowdParlay.Social.Application.Abstractions;

public interface ISubjectsService
{
    public Task<ISet<string>> GetReactionsAsync(string subjectId, Guid authorId);
    public Task SetReactionsAsync(string subjectId, Guid authorId, ISet<string> reactions);
}