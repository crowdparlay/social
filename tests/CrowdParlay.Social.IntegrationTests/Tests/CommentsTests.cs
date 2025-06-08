using CrowdParlay.Social.Domain.DTOs;
using MongoDB.Bson;

namespace CrowdParlay.Social.IntegrationTests.Tests;

public class CommentsTests(WebApplicationContext context) : IAssemblyFixture<WebApplicationContext>
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

    [Theory(DisplayName = "Get replies")]
    [InlineData(true), InlineData(false)]
    public async Task GetReplies(bool flatten)
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

        var discussions = scope.ServiceProvider.GetRequiredService<IDiscussionsService>();
        var comments = scope.ServiceProvider.GetRequiredService<ICommentsService>();

        var authorId1 = Guid.NewGuid();
        var authorId2 = Guid.NewGuid();
        var authorId3 = Guid.NewGuid();
        var authorId4 = Guid.NewGuid();
        var viewerId = Guid.NewGuid();

        var discussion = await discussions.CreateAsync(
            authorId: authorId1,
            title: "Discussion",
            content: "Test discussion.");

        // ReSharper disable UnusedVariable
        // ReSharper disable RedundantAssignment

        var comment1 = await comments.ReplyToDiscussionAsync(discussion.Id, authorId1, "Comment 1");

        var comment11 = await comments.ReplyToCommentAsync(comment1.Id, authorId1, "Comment 11");
        var comment12 = await comments.ReplyToCommentAsync(comment1.Id, authorId1, "Comment 12");

        var comment2 = await comments.ReplyToDiscussionAsync(discussion.Id, authorId1, "Comment 2");

        var comment13 = await comments.ReplyToCommentAsync(comment1.Id, authorId3, "Comment 13");
        var comment14 = await comments.ReplyToCommentAsync(comment1.Id, authorId4, "Comment 14");
        var comment21 = await comments.ReplyToCommentAsync(comment2.Id, authorId3, "Comment 21");

        var comment111 = await comments.ReplyToCommentAsync(comment11.Id, authorId1, "Comment 111");
        var comment112 = await comments.ReplyToCommentAsync(comment11.Id, authorId2, "Comment 112");

        var comment3 = await comments.ReplyToDiscussionAsync(discussion.Id, authorId4, "Comment 3");

        var comment121 = await comments.ReplyToCommentAsync(comment12.Id, authorId4, "Comment 121");

        comment1 = await comments.GetByIdAsync(comment1.Id, viewerId);
        comment11 = await comments.GetByIdAsync(comment11.Id, viewerId);
        comment111 = await comments.GetByIdAsync(comment111.Id, viewerId);
        comment112 = await comments.GetByIdAsync(comment112.Id, viewerId);
        comment12 = await comments.GetByIdAsync(comment12.Id, viewerId);
        comment121 = await comments.GetByIdAsync(comment121.Id, viewerId);
        comment13 = await comments.GetByIdAsync(comment13.Id, viewerId);
        comment14 = await comments.GetByIdAsync(comment14.Id, viewerId);
        comment2 = await comments.GetByIdAsync(comment2.Id, viewerId);
        comment21 = await comments.GetByIdAsync(comment21.Id, viewerId);
        comment3 = await comments.GetByIdAsync(comment3.Id, viewerId);

        // ReSharper restore UnusedVariable
        // ReSharper restore RedundantAssignment

        // Act
        var page = await comments.GetRepliesAsync(discussion.Id, flatten, viewerId, offset: 0, count: 5);

        // Assert
        if (flatten)
        {
            page.TotalCount.Should().Be(11);
            page.Items.Should().HaveCount(5);
            page.Items.Should().BeEquivalentTo([comment1, comment11, comment12, comment2, comment13]);
        }
        else
        {
            page.TotalCount.Should().Be(3);
            page.Items.Should().HaveCount(3);
            page.Items.Should().BeEquivalentTo([comment1, comment2, comment3]);
            page.Items.First().LastCommentsAuthors.Select(author => author.Id).Should().BeEquivalentTo([authorId1, authorId2, authorId4]);
        }
        
        page.Items.First().LastCommentsAuthors.Select(author => author.Id).Should().BeEquivalentTo([authorId1, authorId2, authorId4]);
        page.Items.Should().OnlyContain(comment => comment.ReactionCounters.Count == 0);
        page.Items.Should().OnlyContain(comment => comment.ViewerReactions.Count == 0);
    }

    [Fact(DisplayName = "Get comment with unknown ID")]
    public async Task GetComment_WithUnknownId_ThrowsNotFoundException()
    {
        // Arrange
        await using var scope = _services.CreateAsyncScope();
        var comments = scope.ServiceProvider.GetRequiredService<ICommentsService>();

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
        var discussions = scope.ServiceProvider.GetRequiredService<IDiscussionsService>();
        var comments = scope.ServiceProvider.GetRequiredService<ICommentsService>();

        var authorId = Guid.NewGuid();
        var discussion = await discussions.CreateAsync(authorId, "Discussion", "Test discussion.");
        var comment = await comments.ReplyToDiscussionAsync(discussion.Id, authorId, "Comment content");
        var reply = await comments.ReplyToCommentAsync(comment.Id, authorId, "Reply content");

        // Act
        var page = await comments.GetRepliesAsync(comment.Id, true, authorId, offset: 0, count: 1);

        // Assert
        page.Should().BeEquivalentTo(new Page<CommentResponse>
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