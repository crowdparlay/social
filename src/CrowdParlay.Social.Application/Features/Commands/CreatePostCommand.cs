using CrowdParlay.Social.Application.DTOs;
using CrowdParlay.Social.Application.DTOs.Author;
using CrowdParlay.Social.Application.DTOs.Post;
using FluentValidation;
using MediatR;
using Neo4jClient;
using Neo4jClient.Cypher;

namespace CrowdParlay.Social.Application.Features.Commands;

public record CreatePostCommand(CreatePostDto CreatePostDto) : IRequest<PostDto>;

public class CreatePostHandler : IRequestHandler<CreatePostCommand, PostDto>
{
    private readonly GraphClient _graphClient;
    
    public CreatePostHandler(GraphClient graphClient)
    {
        _graphClient = graphClient;
    }

    public async Task<PostDto> Handle(CreatePostCommand request, CancellationToken cancellationToken)
    {
        var result = await _graphClient.Cypher
            .Match("(a:Author {Id: $AuthorId})")
            .Create("(p:Post {Id: $PostId, Content: $Content, CreatedAt: datetime()})")
            .Create("(p)-[:AUTHORED]->(a)")
            .WithParams(new {
                request.CreatePostDto.AuthorId,
                PostId = Guid.NewGuid(),
                request.CreatePostDto.Content,
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