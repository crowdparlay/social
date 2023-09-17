using CrowdParlay.Social.Application.Exceptions;
using MediatR;
using Neo4jClient;

namespace CrowdParlay.Social.Application.Features.Authors.Commands;

public record DeleteAuthorCommand(Guid Id) : IRequest;

public class DeleteAuthorHandler : IRequestHandler<DeleteAuthorCommand>
{
    private readonly IGraphClient _graphClient;

    public DeleteAuthorHandler(IGraphClient graphClient) => _graphClient = graphClient;

    public async Task Handle(DeleteAuthorCommand request, CancellationToken cancellationToken)
    {
        var results = await _graphClient.Cypher
            .WithParams(new { request.Id })
            .OptionalMatch("(a:Author { Id: $Id })")
            .Delete("a")
            .Return<bool>("COUNT(a) = 0")
            .ResultsAsync;

        if (results.Single())
            throw new NotFoundException();
    }
}