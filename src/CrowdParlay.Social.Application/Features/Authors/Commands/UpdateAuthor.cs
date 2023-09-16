using MediatR;
using Neo4jClient;

namespace CrowdParlay.Social.Application.Features.Authors.Commands;

public record UpdateAuthorCommand(string Id, string Username, string DisplayName, string? AvatarUrl) : IRequest;

public class UpdateAuthorHandler : IRequestHandler<UpdateAuthorCommand>
{
    private readonly GraphClient _graphClient;

    public UpdateAuthorHandler(GraphClient graphClient) => _graphClient = graphClient;

    public async Task<Unit> Handle(UpdateAuthorCommand request, CancellationToken cancellationToken)
    {
        await _graphClient.Cypher
            .WithParams(new
            {
                request.Id,
                request.Username,
                request.DisplayName,
                request.AvatarUrl
            })
            .Match("(a:Author { Id: $Id })")
            .Set(
                """
                a.Username = $Username,
                a.DisplayName = $DisplayName,
                a.AvatarUrl = $AvatarUrl
                """)
            .ExecuteWithoutResultsAsync();

        return Unit.Value;
    }
}