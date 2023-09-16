using CrowdParlay.Social.Application.Exceptions;
using MediatR;
using Neo4jClient;

namespace CrowdParlay.Social.Application.Features.Authors.Commands;

public record DeleteAuthorCommand(Guid Id) : IRequest;

public class DeleteAuthorHandler : IRequestHandler<DeleteAuthorCommand>
{
    private readonly GraphClient _graphClient;

    public DeleteAuthorHandler(GraphClient graphClient) => _graphClient = graphClient;

    public async Task<Unit> Handle(DeleteAuthorCommand request, CancellationToken cancellationToken)
    {
        var results = await _graphClient.Cypher
            .WithParams(new { request.Id })
            .OptionalMatch("(a:Author { Id: $Id })")
            .Delete("a")
            .Return<bool>("COUNT(a) > 0")
            .ResultsAsync;

        return results.Single()
            ? Unit.Value
            : throw new NotFoundException();
    }
}