using CrowdParlay.Social.Application.DTOs.Author;
using CrowdParlay.Social.Application.DTOs.Post;
using FluentValidation;
using MediatR;
using Neo4jClient;

namespace CrowdParlay.Social.Application.Features.Commands;

public sealed record CreatePostCommand(Guid AuthorId, string Content) : IRequest<PostDto>;

internal sealed class CreatePostCommandValidator : AbstractValidator<CreatePostCommand>
{
    public CreatePostCommandValidator()
    {
        RuleFor(post => post.Content)
            .NotEmpty()
            .MinimumLength(5)
            .MaximumLength(500);
    }
}

public class CreatePostHandler : IRequestHandler<CreatePostCommand, PostDto>
{
    private readonly GraphClient _graphClient;
    
    public CreatePostHandler(GraphClient graphClient) => _graphClient = graphClient;

    public async Task<PostDto> Handle(CreatePostCommand request, CancellationToken cancellationToken)
    {
        var queryResult = await _graphClient.Cypher
            .Match("(a:Author {Id: $AuthorId})")
            .Create("(p:Post {Id: $PostId, Content: $Content, CreatedAt: datetime()})")
            .Create("(p)-[:AUTHORED]->(a)")
            .WithParams(new {
                request.AuthorId,
                PostId = Guid.NewGuid(),
                request.Content
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