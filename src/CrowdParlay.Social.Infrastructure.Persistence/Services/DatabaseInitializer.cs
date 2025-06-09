using CrowdParlay.Social.Infrastructure.Persistence.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using static CrowdParlay.Social.Infrastructure.Persistence.MongoDbConstants;

namespace CrowdParlay.Social.Infrastructure.Persistence.Services;

public class DatabaseInitializer(IServiceScopeFactory serviceScopeFactory) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = serviceScopeFactory.CreateAsyncScope();
        var database = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();

        var commentsIndexModels = new CreateIndexModel<CommentDocument>[]
        {
            new(Builders<CommentDocument>.IndexKeys
                .Ascending(comment => comment.SubjectId)
                .Ascending(comment => comment.Id)),
            new(Builders<CommentDocument>.IndexKeys
                .Ascending(comment => comment.AuthorId)
                .Ascending(comment => comment.Id))
        };

        var comments = database.GetCollection<CommentDocument>(Collections.Comments);
        await comments.Indexes.CreateManyAsync(commentsIndexModels, cancellationToken: cancellationToken);

        var discussionsIndexModels = new CreateIndexModel<DiscussionDocument>[]
        {
            new(Builders<DiscussionDocument>.IndexKeys
                .Ascending(comment => comment.AuthorId)
                .Ascending(comment => comment.Id))
        };

        var discussions = database.GetCollection<DiscussionDocument>(Collections.Discussions);
        await discussions.Indexes.CreateManyAsync(discussionsIndexModels, cancellationToken: cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}