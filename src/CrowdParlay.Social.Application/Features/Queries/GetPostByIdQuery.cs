using CrowdParlay.Social.Application.DTOs.Author;
using CrowdParlay.Social.Application.DTOs.Post;
using MediatR;
using Neo4jClient;

namespace CrowdParlay.Social.Application.Features.Queries;

public record GetPostByIdQuery(GetPostByIdDto GetPostByIdDto) : IRequest<PostDto>;

public class GetPostByIdHandler : IRequestHandler<GetPostByIdQuery, PostDto>
{
    private readonly GraphClient _graphClient;

    public GetPostByIdHandler(GraphClient graphClient)
    {
        _graphClient = graphClient;
    }

    public async Task<PostDto> Handle(GetPostByIdQuery request, CancellationToken cancellationToken)
    {
        var result = await _graphClient.Cypher
            .Match("(p:Post {Id: $PostId})-[:AUTHORED]->(a:Author)")
            .WithParams(new
            {
                PostId = request.GetPostByIdDto.Id
            })
            .Return((p, a) => new
            {
                PostDto = p.As<PostDto>(),
                AuthorDto = a.As<AuthorDto>()
            })
            .ResultsAsync;

        var collection = result.Single();
        
        var postWithAuthor = collection.PostDto;
        postWithAuthor.AuthorDto = collection.AuthorDto;

        return postWithAuthor;
    }
}