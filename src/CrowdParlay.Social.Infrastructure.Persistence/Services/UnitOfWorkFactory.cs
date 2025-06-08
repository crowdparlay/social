using CrowdParlay.Social.Domain.Abstractions;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace CrowdParlay.Social.Infrastructure.Persistence.Services;

internal class UnitOfWorkFactory(IMongoClient client, IOptions<MongoDbSettings> settings) : IUnitOfWorkFactory
{
    public async Task<IUnitOfWork> CreateAsync()
    {
        var session = await client.StartSessionAsync();
        session.StartTransaction();
        return new UnitOfWork(session, client.GetDatabase(settings.Value.Database));
    }
}