using CrowdParlay.Social.Application.DTOs.Author;
using MediatR;
using Neo4jClient;

namespace CrowdParlay.Social.Application.Features.Queries;

public record GetAuthorByIdQuery(GetAuthorByIdDto GetAuthorByIdDto) : IRequest<IEnumerable<AuthorDto>>;

public class GetAuthorByIdHandler : IRequestHandler<GetAuthorByIdQuery, IEnumerable<AuthorDto>>
{
    private readonly GraphClient _graphClient;
    
    public GetAuthorByIdHandler(GraphClient graphClient)
    {
        _graphClient = graphClient;
    }
    
    public async Task<IEnumerable<AuthorDto>> Handle(GetAuthorByIdQuery request, CancellationToken cancellationToken)
    {
        var author = await _graphClient.Cypher
            .Match("(a:Author {Id: $Id})")
            .WithParams(
                new
                {
                    Id = request.GetAuthorByIdDto.Id.ToString()
                })
            .Return(a => a.As<AuthorDto>())
            .ResultsAsync;

        return author;
    }
}