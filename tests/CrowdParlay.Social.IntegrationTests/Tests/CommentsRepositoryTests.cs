// ReSharper disable UnusedVariable
// ReSharper disable RedundantAssignment

using CrowdParlay.Social.Domain.Abstractions;
using CrowdParlay.Social.Domain.DTOs;
using CrowdParlay.Social.Domain.Entities;

namespace CrowdParlay.Social.IntegrationTests.Tests;

public class CommentsRepositoryTests(WebApplicationContext context) : IAssemblyFixture<WebApplicationContext>
{
    private readonly IServiceProvider _services = context.Services;

    [Fact(DisplayName = "Create comment")]
    public async Task CreateComment()
    {
        // Arrange
        await using var scope = _services.CreateAsyncScope();

        var discussions = scope.ServiceProvider.GetRequiredService<IDiscussionsRepository>();
        var comments = scope.ServiceProvider.GetRequiredService<ICommentsRepository>();

        var authorId = Guid.NewGuid();
        var discussionId = await discussions.CreateAsync(
            authorId: authorId,
            title: "Discussion",
            description: "Test discussion.");

        // Act
        var commentId = await comments.CreateAsync(authorId, discussionId, "Comment content");
        var comment = await comments.GetByIdAsync(commentId, authorId);

        // Assert
        comment.AuthorId.Should().Be(authorId);
        comment.Content.Should().Be("Comment content");
        comment.ReplyCount.Should().Be(0);
        comment.FirstRepliesAuthorIds.Should().BeEmpty();
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
            description: "Test discussion.");

        var commentId1 = await comments.CreateAsync(authorId1, discussionId, "Comment 1");
        var commentId2 = await comments.CreateAsync(authorId1, discussionId, "Comment 2");
        var commentId3 = await comments.CreateAsync(authorId4, discussionId, "Comment 3");

        var commentId11 = await comments.ReplyToCommentAsync(authorId1, commentId1, "Comment 11");
        var commentId12 = await comments.ReplyToCommentAsync(authorId1, commentId1, "Comment 12");
        var commentId13 = await comments.ReplyToCommentAsync(authorId3, commentId1, "Comment 13");
        var commentId14 = await comments.ReplyToCommentAsync(authorId4, commentId1, "Comment 14");
        var commentId21 = await comments.ReplyToCommentAsync(authorId3, commentId2, "Comment 21");

        var commentId111 = await comments.ReplyToCommentAsync(authorId1, commentId1, "Comment 111");
        var commentId112 = await comments.ReplyToCommentAsync(authorId2, commentId1, "Comment 112");
        var commentId121 = await comments.ReplyToCommentAsync(authorId4, commentId1, "Comment 121");

        var comment1 = await comments.GetByIdAsync(commentId1, viewerId);
        var comment2 = await comments.GetByIdAsync(commentId2, viewerId);
        var comment3 = await comments.GetByIdAsync(commentId3, viewerId);

        // Act
        var page = await comments.SearchAsync(
            discussionId,
            authorId: null,
            viewerId,
            offset: 0,
            count: 2);

        // Assert
        page.TotalCount.Should().Be(3);
        page.Items.Should().HaveCount(2);
        page.Items.Should().BeEquivalentTo([comment1, comment2]);
        page.Items.First().FirstRepliesAuthorIds.Should().BeEquivalentTo([authorId4, authorId2, authorId1]);
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
        Func<Task> getComment = async () => await comments.GetByIdAsync(Guid.NewGuid(), Guid.NewGuid());

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
        var commentId = await comments.CreateAsync(authorId, discussionId, "Comment content");
        var replyId = await comments.ReplyToCommentAsync(authorId, commentId, "Reply content");
        var reply = await comments.GetByIdAsync(replyId, authorId);

        // Act
        var page = await comments.SearchAsync(commentId, authorId: null, authorId, offset: 0, count: 1);

        // Assert
        page.Should().BeEquivalentTo(new Page<Comment>
        {
            TotalCount = 1,
            Items = [reply]
        });
    }

    [Fact(DisplayName = "Create comment with unknown author and discussion")]
    public async Task CreateComment_WithUnknownAuthorAndDiscussion_ThrowsNotFoundException()
    {
        // Arrange
        await using var scope = _services.CreateAsyncScope();
        var comments = scope.ServiceProvider.GetRequiredService<ICommentsRepository>();

        // Act
        Func<Task> createComment = async () =>
            await comments.CreateAsync(Guid.NewGuid(), Guid.NewGuid(), "Comment content");

        // Assert
        await createComment.Should().ThrowAsync<NotFoundException>();
    }
}