using CrowdParlay.Social.Domain.Abstractions;
using MongoDB.Driver;

namespace CrowdParlay.Social.Infrastructure.Persistence.Services;

public class UnitOfWork(IClientSessionHandle session, IMongoDatabase database) : IUnitOfWork
{
    private readonly Lazy<DiscussionsRepository> _discussionsRepository = new(() => new(session, database));
    public IDiscussionsRepository DiscussionsRepository => _discussionsRepository.Value;

    private readonly Lazy<CommentsRepository> _commentsRepository = new(() => new(session, database));
    public ICommentsRepository CommentsRepository => _commentsRepository.Value;

    public async Task CommitAsync() => await session.CommitTransactionAsync();
    public async Task RollbackAsync() => await session.AbortTransactionAsync();
    public void Dispose() => session.Dispose();
}