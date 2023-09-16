using CrowdParlay.Social.Application.DTOs.Author;
using MediatR;
using Neo4jClient;

namespace CrowdParlay.Social.Application.Features.Authors.Commands;

public sealed record CreateAuthorCommand(Guid Id, string Username, string DisplayName, string? AvatarUrl) : IRequest<AuthorDto>;

public class CreateAuthorHandler : IRequestHandler<CreateAuthorCommand, AuthorDto>
{
    private readonly GraphClient _graphClient;

    public CreateAuthorHandler(GraphClient graphClient) => _graphClient = graphClient;

    public async Task<AuthorDto> Handle(CreateAuthorCommand request, CancellationToken cancellationToken)
    {
        var author = await _graphClient.Cypher
            .WithParams(new
            {
                Id = request.Id.ToString(),
                request.Username,
                request.DisplayName,
                request.AvatarUrl
            })
            .Create(
                """
                (a:Author {
                    Id: $Id,
                    Username: $Username,
                    DisplayName: $DisplayName,
                    AvatarUrl: $AvatarUrl
                })
                """)
            .Return<AuthorDto>("a")
            .ResultsAsync;

        return author.Single();
    }
}