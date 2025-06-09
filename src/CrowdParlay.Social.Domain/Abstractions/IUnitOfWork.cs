namespace CrowdParlay.Social.Domain.Abstractions;

public interface IUnitOfWork : IDisposable
{
    public IDiscussionsRepository DiscussionsRepository { get; }
    public ICommentsRepository CommentsRepository { get; }

    Task CommitAsync();
    Task RollbackAsync();
}