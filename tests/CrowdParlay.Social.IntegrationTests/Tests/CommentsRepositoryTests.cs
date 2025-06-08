// ReSharper disable UnusedVariable
// ReSharper disable RedundantAssignment

using CrowdParlay.Social.Domain.Abstractions;
using CrowdParlay.Social.Domain.DTOs;
using CrowdParlay.Social.Domain.Entities;
using MongoDB.Bson;

namespace CrowdParlay.Social.IntegrationTests.Tests;

public class CommentsRepositoryTests(WebApplicationContext context) : IAssemblyFixture<WebApplicationContext>
{
    private readonly IServiceProvider _services = context.Services;

    [Fact(DisplayName = "Create comment")]
    public async Task CreateComment()
    {
        // Arrange
        await using var scope = _services.CreateAsyncScope();

        var discussions = scope.ServiceProvider.GetRequiredService<IDiscussionsService>();
        var comments = scope.ServiceProvider.GetRequiredService<ICommentsService>();

        var authorId = Guid.NewGuid();
        var discussion = await discussions.CreateAsync(
            authorId: authorId,
            title: "Discussion",
            content: "Test discussion.");
        
        // Act
        var comment = await comments.ReplyToDiscussionAsync(discussion.Id, authorId, "Comment content");
        await comments.ReplyToCommentAsync(comment.Id, authorId, "Reply content");
        comment = await comments.GetByIdAsync(comment.Id, authorId);

        // Assert
        comment.Author!.Id.Should().Be(authorId);
        comment.Content.Should().Be("Comment content");
        comment.CommentCount.Should().Be(1);
        comment.LastCommentsAuthors.Should().ContainSingle().Which.Id.Should().Be(authorId);
        comment.CreatedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMinutes(1));
        comment.ReactionCounters.Should().BeEmpty();
        comment.ViewerReactions.Should().BeEmpty();
    }

    [Fact(DisplayName = "Search comments")]
    public async Task SearchComments()
    {
        /*
        ┌───────────────────────┬───────────────────────┐
        │  COMMENT              │  AUTHOR               │
        ├───────────────────────┼───────────────────────┤
        │  comment 1            │  author 1             │
        │   • comment 11        │  author 1             │
        │      • comment 111    │  author 1             │
        │      • comment 112    │  author 2             │
        │   • comment 12        │  author 1             │
        │      • comment 121    │  author 4             │
        │   • comment 13        │  author 3             │
        │   • comment 14        │  author 4             │
        │  comment 2            │  author 1             │
        │   • comment 21        │  author 3             │
        │  comment 3            │  author 4             │
        └───────────────────────┴───────────────────────┘
        */

        // Arrange
        await using var scope = _services.CreateAsyncScope();

        var discussions = scope.ServiceProvider.GetRequiredService<IDiscussionsRepository>();
        var comments = scope.ServiceProvider.GetRequiredService<ICommentsRepository>();

        var authorId1 = Guid.NewGuid();
        var authorId2 = Guid.NewGuid();
        var authorId3 = Guid.NewGuid();
        var authorId4 = Guid.NewGuid();
        var viewerId = Guid.NewGuid();

        var discussionId = await discussions.CreateAsync(
            authorId: authorId1,
            title: "Discussion",
            content: "Test discussion.");

        var commentId1 = await comments.CreateAsync(discussionId, authorId1, "Comment 1");
        var commentId2 = await comments.CreateAsync(discussionId, authorId1, "Comment 2");
        var commentId3 = await comments.CreateAsync(discussionId, authorId4, "Comment 3");

        var commentId11 = await comments.CreateAsync(commentId1, authorId1, "Comment 11");
        var commentId12 = await comments.CreateAsync(commentId1, authorId1, "Comment 12");
        var commentId13 = await comments.CreateAsync(commentId1, authorId3, "Comment 13");
        var commentId14 = await comments.CreateAsync(commentId1, authorId4, "Comment 14");
        var commentId21 = await comments.CreateAsync(commentId2, authorId3, "Comment 21");

        var commentId111 = await comments.CreateAsync(commentId1, authorId1, "Comment 111");
        var commentId112 = await comments.CreateAsync(commentId1, authorId2, "Comment 112");
        var commentId121 = await comments.CreateAsync(commentId1, authorId4, "Comment 121");

        var comment1 = await comments.GetByIdAsync(commentId1, viewerId);
        var comment2 = await comments.GetByIdAsync(commentId2, viewerId);
        var comment3 = await comments.GetByIdAsync(commentId3, viewerId);

        // Act
        var page = await comments.GetRepliesAsync(
            discussionId,
            flatten: false,
            viewerId,
            offset: 0,
            count: 2);

        // Assert
        page.TotalCount.Should().Be(3);
        page.Items.Should().HaveCount(2);
        page.Items.Should().BeEquivalentTo([comment1, comment2]);
        //page.Items.First().LastRepliesAuthorIds.Should().BeEquivalentTo([authorId4, authorId2, authorId1]);
        page.Items.Should().OnlyContain(comment => comment.ReactionCounters.Count == 0);
        page.Items.Should().OnlyContain(comment => comment.ViewerReactions.Count == 0);
    }

    [Fact(DisplayName = "Get comment with unknown ID")]
    public async Task GetComment_WithUnknownId_ThrowsNotFoundException()
    {
        // Arrange
        await using var scope = _services.CreateAsyncScope();
        var comments = scope.ServiceProvider.GetRequiredService<ICommentsRepository>();

        // Act
        var getComment = async () =>
            await comments.GetByIdAsync(ObjectId.GenerateNewId().ToString(), Guid.NewGuid());

        // Assert
        await getComment.Should().ThrowAsync<NotFoundException>();
    }

    [Fact(DisplayName = "Get replies to comment")]
    public async Task GetRepliesToComment()
    {
        // Arrange
        await using var scope = _services.CreateAsyncScope();
        var discussions = scope.ServiceProvider.GetRequiredService<IDiscussionsRepository>();
        var comments = scope.ServiceProvider.GetRequiredService<ICommentsRepository>();

        var authorId = Guid.NewGuid();
        var discussionId = await discussions.CreateAsync(authorId, "Discussion", "Test discussion.");
        var commentId = await comments.CreateAsync(discussionId, authorId, "Comment content");
        var replyId = await comments.CreateAsync(commentId, authorId, "Reply content");
        var reply = await comments.GetByIdAsync(replyId, authorId);

        // Act
        var page = await comments.GetRepliesAsync(commentId, true, authorId, offset: 0, count: 1);

        // Assert
        page.Should().BeEquivalentTo(new Page<Comment>
        {
            TotalCount = 1,
            Items = [reply]
        });
    }

    [Fact(DisplayName = "Create comment with unknown discussion")]
    public async Task CreateComment_WithUnknownDiscussion_ThrowsNotFoundException()
    {
        // Arrange
        await using var scope = _services.CreateAsyncScope();
        var comments = scope.ServiceProvider.GetRequiredService<ICommentsService>();

        // Act
        var createComment = async () =>
            await comments.ReplyToDiscussionAsync(ObjectId.GenerateNewId().ToString(), Guid.NewGuid(), "Comment content");

        // Assert
        await createComment.Should().ThrowAsync<NotFoundException>();
    }
}