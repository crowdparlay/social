using System.ComponentModel.DataAnnotations;

namespace CrowdParlay.Social.Infrastructure.Persistence;

internal record MongoDbSettings
{
    [Required] public required string ConnectionString { get; init; }
    [Required] public required string Database { get; init; }
}