using CrowdParlay.Social.Application.Exceptions;
using Microsoft.AspNetCore.SignalR;

namespace CrowdParlay.Social.Api.Hubs;

public class CommentsHub : Hub
{
    private const string DiscussionIdQueryParameterName = "discussionId";

    public override async Task OnConnectedAsync()
    {
        var discussionId = GetDiscussionIdFromQuery();
        await Groups.AddToGroupAsync(Context.ConnectionId, GroupNames.NewCommentInDiscussion(discussionId));
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var discussionId = GetDiscussionIdFromQuery();
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupNames.NewCommentInDiscussion(discussionId));
    }

    private string GetDiscussionIdFromQuery()
    {
        var query =
            Context.GetHttpContext()?.Request.Query
            ?? throw new InvalidOperationException(
                "Cannot access request's query parameters, since HttpContext is null.");

        return query.TryGetValue(DiscussionIdQueryParameterName, out var values)
            ? values.SingleOrDefault() ??
              throw new ValidationException(DiscussionIdQueryParameterName, ["Must have single value."])
            : throw new ValidationException(DiscussionIdQueryParameterName, ["Query parameter is required."]);
    }

    public static class GroupNames
    {
        public static string NewCommentInDiscussion(string discussionId) =>
            $"{Events.NewComment.ToString()}/{discussionId}";
    }

    public enum Events
    {
        NewComment
    }
}