using CrowdParlay.Social.Application.DTOs.Author;
using CrowdParlay.Social.Application.DTOs.Post;
using MediatR;
using Neo4jClient;

namespace CrowdParlay.Social.Application.Features.Posts.Queries;

public record GetAllPostsQuery(int Offset, int Limit) : IRequest<IEnumerable<PostDto>>;

public class GetAllPostsHandler : IRequestHandler<GetAllPostsQuery, IEnumerable<PostDto>>
{
    private readonly GraphClient _graphClient;

    public GetAllPostsHandler(GraphClient graphClient) => _graphClient = graphClient;

    public async Task<IEnumerable<PostDto>> Handle(GetAllPostsQuery request, CancellationToken cancellationToken)
    {
        var posts = await _graphClient.Cypher
            .Match("(p:Post)-[:AUTHORED]->(a:Author)")
            .Return((p, a) => new PostDto
            {
                Id = p.As<PostDto>().Id,
                Content = p.As<PostDto>().Content,
                CreatedAt = p.As<PostDto>().CreatedAt,
                AuthorDto = a.As<AuthorDto>()
            })
            .Skip(request.Offset)
            .Limit(request.Limit)
            .ResultsAsync;

        return posts;
    }
}