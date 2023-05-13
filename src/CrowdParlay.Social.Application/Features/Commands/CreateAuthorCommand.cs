using CrowdParlay.Social.Application.DTOs.Author;
using MediatR;
using Neo4jClient;

namespace CrowdParlay.Social.Application.Features.Commands;

public record CreateAuthorCommand(CreateAuthorDto CreateAuthor) : IRequest<CreateAuthorDto>;

public class CreateAuthorHandler : IRequestHandler<CreateAuthorCommand, CreateAuthorDto>
{
    private readonly GraphClient _graphClient;

    public CreateAuthorHandler(GraphClient graphClient)
    {
        _graphClient = graphClient;
    }
    
    public async Task<CreateAuthorDto> Handle(CreateAuthorCommand request, CancellationToken cancellationToken)
    {
        var author = await _graphClient.Cypher
            .Create(@"(a:Author {Id: $Id, 
                                    DisplayName: $DisplayName, 
                                    AvatarUrl: $AvatarUrl, 
                                    Alias: $Alias})")
            .WithParams(
                new
                {
                    Id = request.CreateAuthor.Id.ToString(),
                    request.CreateAuthor.DisplayName,
                    request.CreateAuthor.AvatarUrl,
                    request.CreateAuthor.Alias
                })
            .Return(a => a.As<CreateAuthorDto>())
            .ResultsAsync;

        return author.Single();
    }
}