using CrowdParlay.Social.Application.DTOs.Post;
using MediatR;
using Neo4jClient;

namespace CrowdParlay.Social.Application.Features.Queries;

public record GetAllPostsQuery(GetAllPostsDto GetAllPostsDto) : IRequest<IEnumerable<PostDto>>;

public class GetAllPostsHandler : IRequestHandler<GetAllPostsQuery, IEnumerable<PostDto>>
{
    private readonly GraphClient _graphClient;

    public GetAllPostsHandler(GraphClient graphClient)
    {
        _graphClient = graphClient;
    }

    public async Task<IEnumerable<PostDto>> Handle(GetAllPostsQuery request, CancellationToken cancellationToken)
    {
        var posts = await _graphClient.Cypher
            .Match("(p:Post)-[:AUTHORED]->(a:Author)")
            .Skip(request.GetAllPostsDto.Offset)
            .Limit(request.GetAllPostsDto.Limit)
            .Return(p => p.As<PostDto>())
            .ResultsAsync;

        return posts;
    }
}