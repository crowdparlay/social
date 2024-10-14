using CrowdParlay.Social.Domain.Abstractions;
using Neo4j.Driver;

namespace CrowdParlay.Social.Infrastructure.Persistence.Services;

public class UnitOfWorkFactory(IAsyncSession session) : IUnitOfWorkFactory
{
    public async Task<IUnitOfWork> CreateAsync()
    {
        var transaction = await session.BeginTransactionAsync();
        return new UnitOfWork(transaction);
    }
}