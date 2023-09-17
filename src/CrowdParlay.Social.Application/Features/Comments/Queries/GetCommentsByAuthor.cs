using CrowdParlay.Social.Application.DTOs.Comment;
using MediatR;
using Neo4jClient;

namespace CrowdParlay.Social.Application.Features.Comments.Queries;

public record GetCommentsByAuthorQuery(Guid AuthorId) : IRequest<IEnumerable<CommentDto>>;

public class GetCommentsByAuthorHandler : IRequestHandler<GetCommentsByAuthorQuery, IEnumerable<CommentDto>>
{
    private readonly IGraphClient _graphClient;

    public GetCommentsByAuthorHandler(IGraphClient graphClient) => _graphClient = graphClient;

    public async Task<IEnumerable<CommentDto>> Handle(GetCommentsByAuthorQuery request, CancellationToken cancellationToken) =>
        await _graphClient.Cypher
            .WithParams(new { request.AuthorId })
            .Match("(c:Comment)-[:AUTHORED_BY]->(a:Author { Id: $AuthorId })")
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
                    ReplyCount: rc,
                    FirstRepliesAuthors: fras
                }
                """)
            .ResultsAsync;
}