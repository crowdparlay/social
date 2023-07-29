using CrowdParlay.Social.Application.DTOs.Author;
using MediatR;
using Neo4jClient;

namespace CrowdParlay.Social.Application.Features.Authors.Commands;

public sealed record CreateAuthorCommand(Guid Id, string DisplayName, string AvatarUrl, string? Alias) : IRequest<AuthorDto>;

public class CreateAuthorHandler : IRequestHandler<CreateAuthorCommand, AuthorDto>
{
    private readonly GraphClient _graphClient;

    public CreateAuthorHandler(GraphClient graphClient) => _graphClient = graphClient;
    
    public async Task<AuthorDto> Handle(CreateAuthorCommand request, CancellationToken cancellationToken)
    {
        var author = await _graphClient.Cypher
            .Create(
                @"(a:Author {
                        Id: $Id, 
                        DisplayName: $DisplayName, 
                        AvatarUrl: $AvatarUrl, 
                        Alias: $Alias
                })")
            .WithParams(
                new
                {
                    AuthorId = request.Id.ToString(),
                    request.DisplayName,
                    request.AvatarUrl,
                    request.Alias
                })
            .Return(a => a.As<AuthorDto>())
            .ResultsAsync;

        return author.Single();
    }
}