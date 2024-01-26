namespace CrowdParlay.Social.Api.v1.DTOs;

public record CommentRequest(Guid DiscussionId, string Content);