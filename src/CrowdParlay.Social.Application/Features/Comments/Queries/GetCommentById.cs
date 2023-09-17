using CrowdParlay.Social.Application.DTOs.Comment;
using CrowdParlay.Social.Application.Exceptions;
using MediatR;
using Neo4jClient;

namespace CrowdParlay.Social.Application.Features.Comments.Queries;

public record GetCommentByIdQuery(Guid CommentId) : IRequest<CommentDto>;

public class GetCommentByIdHandler : IRequestHandler<GetCommentByIdQuery, CommentDto>
{
    private readonly IGraphClient _graphClient;

    public GetCommentByIdHandler(IGraphClient graphClient) => _graphClient = graphClient;

    public async Task<CommentDto> Handle(GetCommentByIdQuery request, CancellationToken cancellationToken)
    {
        var results = await _graphClient.Cypher
            .WithParams(new { request.CommentId })
            .Match("(c:Comment { Id: $CommentId })-[:AUTHORED_BY]->(a:Author)")
            .OptionalMatch("(ra:Author)<-[:AUTHORED_BY]-(r:Comment)-[:REPLIES_TO]->(c)")
            .With(
                """
                c, a, COUNT(r) AS rc,
                CASE WHEN COUNT(r) > 0 THEN COLLECT(DISTINCT {
                    Id: ra.Id,
                    Username: ra.Username,
                    DisplayName: ra.DisplayName,
                    AvatarUrl: ra.AvatarUrl
                })[0..3] ELSE [] END AS fras
                """)
            .Return<CommentDto>(
                """
                {
                    Id: c.Id,
                    Content: c.content,
                    Author: {
                        Id: a.Id,
                        Username: a.Username,
                        DisplayName: a.DisplayName,
                        AvatarUrl: a.AvatarUrl
                    },
                    CreatedAt: c.CreatedAt,
                    ReplyCount: rc,
                    FirstRepliesAuthors: fras
                }
                """)
            .ResultsAsync;

        return
            results.SingleOrDefault()
            ?? throw new NotFoundException();
    }
}