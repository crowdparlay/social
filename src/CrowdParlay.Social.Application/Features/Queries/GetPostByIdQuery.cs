using CrowdParlay.Social.Application.DTOs.Author;
using CrowdParlay.Social.Application.DTOs.Post;
using MediatR;
using Neo4jClient;

namespace CrowdParlay.Social.Application.Features.Queries;

public record GetPostByIdQuery(Guid Id) : IRequest<PostDto>;

public class GetPostByIdHandler : IRequestHandler<GetPostByIdQuery, PostDto>
{
    private readonly GraphClient _graphClient;

    public GetPostByIdHandler(GraphClient graphClient)
    {
        _graphClient = graphClient;
    }

    public async Task<PostDto> Handle(GetPostByIdQuery request, CancellationToken cancellationToken)
    {
        var queryResult = await _graphClient.Cypher
            .Match("(p:Post {Id: $PostId})-[:AUTHORED]->(a:Author)")
            .WithParams(new
            {
                PostId = request.Id
            })
            .Return((p, a) => new
            {
                PostDto = p.As<PostDto>(),
                AuthorDto = a.As<AuthorDto>()
            })
            .ResultsAsync;

        var result = queryResult.Single();
        
        var postWithAuthor = result.PostDto;
        postWithAuthor.AuthorDto = result.AuthorDto;

        return postWithAuthor;
    }
}