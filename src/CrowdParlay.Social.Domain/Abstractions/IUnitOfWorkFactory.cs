namespace CrowdParlay.Social.Domain.Abstractions;

public interface IUnitOfWorkFactory
{
    public Task<IUnitOfWork> CreateAsync();
}