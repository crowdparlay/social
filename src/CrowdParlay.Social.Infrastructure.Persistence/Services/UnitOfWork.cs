using CrowdParlay.Social.Domain.Abstractions;
using Neo4j.Driver;

namespace CrowdParlay.Social.Infrastructure.Persistence.Services;

public class UnitOfWork(IAsyncTransaction transaction) : IUnitOfWork
{
    private readonly Lazy<DiscussionsRepository> _discussionsRepository = new(() => new(transaction));
    public IDiscussionsRepository DiscussionsRepository => _discussionsRepository.Value;

    private readonly Lazy<CommentsRepository> _commentsRepository = new(() => new(transaction));
    public ICommentsRepository CommentsRepository => _commentsRepository.Value;

    private readonly Lazy<ReactionsRepository> _reactionsRepository = new(() => new(transaction));
    public IReactionsRepository ReactionsRepository => _reactionsRepository.Value;

    public async Task CommitAsync() => await transaction.CommitAsync();
    public async Task RollbackAsync() => await transaction.RollbackAsync();
    public async ValueTask DisposeAsync() => await transaction.DisposeAsync();
}