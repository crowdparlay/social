using CrowdParlay.Social.Application.DTOs.Comment;
using FluentValidation;
using MediatR;
using Neo4jClient;

namespace CrowdParlay.Social.Application.Features.Comments.Commands;

public sealed record CreateCommentCommand(Guid AuthorId, string Content) : IRequest<CommentDto>;

internal sealed class CreateCommentCommandValidator : AbstractValidator<CreateCommentCommand>
{
    public CreateCommentCommandValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty()
            .MinimumLength(5)
            .MaximumLength(500);
    }
}

public class CreateCommentHandler : IRequestHandler<CreateCommentCommand, CommentDto>
{
    private readonly GraphClient _graphClient;

    public CreateCommentHandler(GraphClient graphClient) => _graphClient = graphClient;

    public async Task<CommentDto> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
    {
        var result = await _graphClient.Cypher
            .WithParams(new
            {
                request.AuthorId,
                request.Content
            })
            .Match("(a:Author {Id: $AuthorId})")
            .Create(
                """
                (c:Comment {
                    Id: randomUUID(),
                    Content: $Content,
                    CreatedAt: datetime()
                })
                """)
            .Create("(c)-[:AUTHORED_BY]->(a)")
            .Return<CommentDto>("c")
            .ResultsAsync;

        return result.Single();
    }
}