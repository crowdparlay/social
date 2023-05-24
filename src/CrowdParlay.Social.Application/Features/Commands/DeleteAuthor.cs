using CrowdParlay.Social.Application.DTOs.Author;
using MediatR;
using Neo4jClient;

namespace CrowdParlay.Social.Application.Features.Commands;

public record DeleteAuthorCommand(Guid Id) : IRequest;

public class DeleteAuthorHandler : IRequestHandler<DeleteAuthorCommand>
{
    private readonly GraphClient _graphClient;

    public DeleteAuthorHandler(GraphClient graphClient)
    {
        _graphClient = graphClient;
    }

    public async Task<Unit> Handle(DeleteAuthorCommand request, CancellationToken cancellationToken)
    {
        await _graphClient.Cypher
            .Match("(a:Author {Id: $Id})")
            .WithParams(new { request.Id })
            .Delete("a")
            .ExecuteWithoutResultsAsync();

        return Unit.Value;
    }
}