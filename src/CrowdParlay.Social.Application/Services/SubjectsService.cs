using CrowdParlay.Social.Application.Abstractions;
using CrowdParlay.Social.Application.Exceptions;
using CrowdParlay.Social.Domain.Abstractions;
using CrowdParlay.Social.Domain.ValueObjects;

namespace CrowdParlay.Social.Application.Services;

public class SubjectsService(ISubjectsRepository subjectsRepository) : ISubjectsService
{
    public async Task<ISet<string>> GetReactionsAsync(string subjectId, Guid authorId) =>
        await subjectsRepository.GetReactionsAsync(subjectId, authorId);

    public async Task SetReactionsAsync(string subjectId, Guid authorId, ISet<string> newReactions)
    {
        var oldReactions = await subjectsRepository.GetReactionsAsync(subjectId, authorId);
        var allowedReactions = Reaction.AllowedValues.Union(oldReactions).ToArray();

        if (!newReactions.IsSubsetOf(allowedReactions))
            throw new ForbiddenException("Such reaction set is not allowed.");

        await subjectsRepository.SetReactionsAsync(subjectId, authorId, newReactions);
        
        var reactionsToAdd = newReactions.Except(oldReactions).ToArray();
        var reactionsToRemove = oldReactions.Except(newReactions).ToArray();
        await subjectsRepository.UpdateReactionCountersAsync(subjectId, reactionsToAdd, reactionsToRemove);
    }
}