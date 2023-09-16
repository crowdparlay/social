using CrowdParlay.Social.Application.DTOs.Author;
using CrowdParlay.Social.Application.Exceptions;
using MediatR;
using Neo4jClient;

namespace CrowdParlay.Social.Application.Features.Authors.Queries;

public record GetAuthorByIdQuery(Guid Id) : IRequest<AuthorDto>;

public class GetAuthorByIdHandler : IRequestHandler<GetAuthorByIdQuery, AuthorDto>
{
    private readonly GraphClient _graphClient;

    public GetAuthorByIdHandler(GraphClient graphClient) => _graphClient = graphClient;

    public async Task<AuthorDto> Handle(GetAuthorByIdQuery request, CancellationToken cancellationToken)
    {
        var results = await _graphClient.Cypher
            .WithParams(new { request.Id })
            .Match("(a:Author { Id: $Id })")
            .Return<AuthorDto>("a")
            .ResultsAsync;

        return
            results.SingleOrDefault()
            ?? throw new NotFoundException();
    }
}