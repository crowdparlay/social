using CrowdParlay.Social.Application.Abstractions;
using CrowdParlay.Social.Application.Exceptions;
using CrowdParlay.Social.Domain.Abstractions;
using CrowdParlay.Social.Domain.ValueObjects;

namespace CrowdParlay.Social.Application.Services;

public class ReactionsService(IUnitOfWorkFactory unitOfWorkFactory, IReactionsRepository reactionsRepository) : IReactionsService
{
    public async Task<ISet<string>> GetAsync(Guid subjectId, Guid viewerId) =>
        await reactionsRepository.GetAsync(subjectId, viewerId);

    public async Task SetAsync(Guid subjectId, Guid viewerId, ISet<string> reactions)
    {
        await using var unitOfWork = await unitOfWorkFactory.CreateAsync();

        var alreadyExistingSubjectReactions = await unitOfWork.ReactionsRepository.GetAsync(subjectId);
        var allowedReactions = Reaction.AllowedValues.Union(alreadyExistingSubjectReactions).ToHashSet();

        if (!reactions.IsSubsetOf(allowedReactions))
            throw new ForbiddenException("Such reaction set is not allowed.");

        await unitOfWork.ReactionsRepository.SetAsync(subjectId, viewerId, reactions);
        await unitOfWork.CommitAsync();
    }
}