namespace CrowdParlay.Social.Application.DTOs;

public record CommentRequest(Guid DiscussionId, string Content);