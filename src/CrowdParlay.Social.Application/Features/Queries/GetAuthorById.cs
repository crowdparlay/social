using CrowdParlay.Social.Application.DTOs.Author;
using MediatR;
using Neo4jClient;

namespace CrowdParlay.Social.Application.Features.Queries;

public record GetAuthorByIdQuery(Guid Id) : IRequest<AuthorDto>;

public class GetAuthorByIdHandler : IRequestHandler<GetAuthorByIdQuery, AuthorDto>
{
    private readonly GraphClient _graphClient;
    
    public GetAuthorByIdHandler(GraphClient graphClient) => _graphClient = graphClient;
    
    public async Task<AuthorDto> Handle(GetAuthorByIdQuery request, CancellationToken cancellationToken)
    {
        var author = await _graphClient.Cypher
            .Match("(a:Author {Id: $Id})")
            .WithParams(new
            {
                Id = request.Id.ToString()
            })
            .Return(a => a.As<AuthorDto>())
            .ResultsAsync;

        return author.Single();
    }
}