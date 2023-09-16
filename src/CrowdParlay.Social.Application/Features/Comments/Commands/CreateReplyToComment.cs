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
    private readonly GraphClient _graphClient;

    public ReplyToCommentHandler(GraphClient graphClient) => _graphClient = graphClient;

    public async Task<CommentDto> Handle(CreateReplyToCommentCommand request, CancellationToken cancellationToken)
    {
        var result = await _graphClient.Cypher
            .WithParams(new
            {
                CommentId = Guid.NewGuid(),
                request.AuthorId,
                request.Content,
                request.InReplyToCommentId
            })
            .Match("(a:Author {Id: $AuthorId})")
            .Match("(t:Comment {Id: $InReplyToCommentId})")
            .Create("(c:Comment {Id: $CommentId, Content: $Content, CreatedAt: datetime()})")
            .Create("(t)<-[:REPLIES_TO]-(c)-[:AUTHORED_BY]->(a)")
            .Return(c => c.As<CommentDto>())
            .ResultsAsync;

        return result.Single();
    }
}