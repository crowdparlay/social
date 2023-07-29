using CrowdParlay.Social.Application.DTOs.Author;
using MediatR;
using Neo4jClient;

namespace CrowdParlay.Social.Application.Features.Authors.Commands;

public record UpdateAuthorCommand(string UserId, string Username, string DisplayName, string? AvatarUrl) : IRequest;

public class UpdateAuthorHandler : IRequestHandler<UpdateAuthorCommand>
{
    private readonly GraphClient _graphClient;

    public UpdateAuthorHandler(GraphClient graphClient) => _graphClient = graphClient;

    public async Task<Unit> Handle(UpdateAuthorCommand request, CancellationToken cancellationToken)
    {
        await _graphClient.Cypher
            .Match("(a:Author)")
            .Where((AuthorDto a) => a.Id.ToString() == request.UserId)
            .Set("a.DisplayName = $DisplayName, a.Alias = $Alias, a.AvatarUrl = $AvatarUrl")
            .WithParams(
                new
                {
                    request.DisplayName,
                    Alias = request.Username,
                    request.AvatarUrl
                })
            .ExecuteWithoutResultsAsync();

        return Unit.Value;
    }
}