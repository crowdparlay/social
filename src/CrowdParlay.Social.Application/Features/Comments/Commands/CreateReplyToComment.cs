using CrowdParlay.Social.Application.DTOs.Comment;
using FluentValidation;
using MediatR;
using Neo4jClient;

namespace CrowdParlay.Social.Application.Features.Comments.Commands;

public sealed record CreateReplyToCommentCommand(Guid AuthorId, string Content, Guid InReplyToCommentId) : IRequest<CommentDto>;

internal sealed class CreateReplyToCommentCommandValidator : AbstractValidator<CreateReplyToCommentCommand>
{
    public CreateReplyToCommentCommandValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty()
            .MinimumLength(5)
            .MaximumLength(500);
    }
}

public class ReplyToCommentHandler : IRequestHandler<CreateReplyToCommentCommand, CommentDto>
{
    private readonly IGraphClient _graphClient;

    public ReplyToCommentHandler(IGraphClient graphClient) => _graphClient = graphClient;

    public async Task<CommentDto> Handle(CreateReplyToCommentCommand request, CancellationToken cancellationToken)
    {
        var result = await _graphClient.Cypher
            .WithParams(new
            {
                request.AuthorId,
                request.Content,
                request.InReplyToCommentId
            })
            .Match("(a:Author {Id: $AuthorId})")
            .Match("(t:Comment {Id: $InReplyToCommentId})")
            .Create(
                """
                (c:Comment {
                    Id: randomUUID(),
                    Content: $Content,
                    CreatedAt: datetime()
                })
                """)
            .Create("(t)<-[:REPLIES_TO]-(c)-[:AUTHORED_BY]->(a)")
            .Return<CommentDto>("c")
            .ResultsAsync;

        return result.Single();
    }
}