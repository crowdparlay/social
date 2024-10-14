namespace CrowdParlay.Social.Domain.Abstractions;

public interface IUnitOfWork : IAsyncDisposable
{
    public IDiscussionsRepository DiscussionsRepository { get; }
    public ICommentsRepository CommentsRepository { get; }
    public IReactionsRepository ReactionsRepository { get; }

    Task CommitAsync();
    Task RollbackAsync();
}