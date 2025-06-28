using CrowdParlay.Social.Application.Abstractions;
using CrowdParlay.Social.Application.Exceptions;
using CrowdParlay.Social.Aspects;
using CrowdParlay.Social.Domain.Abstractions;
using CrowdParlay.Social.Domain.ValueObjects;
using Metalama.Framework.Code;

namespace CrowdParlay.Social.Application.Services;

[TraceMethods(Accessibility.Public, Accessibility.Private)]
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

        var addedReactionsDiff = newReactions.Except(oldReactions).Select(reaction => new KeyValuePair<string, int>(reaction, 1));
        var removedReactionsDiff = oldReactions.Except(newReactions).Select(reaction => new KeyValuePair<string, int>(reaction, -1));
        var reactionsDiff = addedReactionsDiff.Concat(removedReactionsDiff).ToDictionary();
        await subjectsRepository.UpdateReactionCountersAsync(subjectId, reactionsDiff);
    }
}