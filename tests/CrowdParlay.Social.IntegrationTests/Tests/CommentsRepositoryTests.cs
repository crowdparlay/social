// ReSharper disable UnusedVariable
// ReSharper disable RedundantAssignment

using CrowdParlay.Social.Domain.Abstractions;
using CrowdParlay.Social.Domain.DTOs;
using Mapster;

namespace CrowdParlay.Social.IntegrationTests.Tests;

public class CommentsRepositoryTests(WebApplicationContext context) : IClassFixture<WebApplicationContext>
{
    private readonly IServiceProvider _services = context.Services;

    [Fact(DisplayName = "Create comment")]
    public async Task CreateComment()
    {
        // Arrange
        await using var scope = _services.CreateAsyncScope();

        var discussions = scope.ServiceProvider.GetRequiredService<IDiscussionsRepository>();
        var comments = scope.ServiceProvider.GetRequiredService<ICommentRepository>();

        var authorId = Guid.NewGuid();
        var discussion = await discussions.CreateAsync(
            authorId: authorId,
            title: "Discussion",
            description: "Test discussion.");

        // Act
        var comment = await comments.CreateAsync(authorId, discussion.Id, "Comment content");

        // Assert
        comment.AuthorId.Should().Be(authorId);
        comment.Content.Should().Be("Comment content");
        comment.ReplyCount.Should().Be(0);
        comment.FirstRepliesAuthorIds.Should().BeEmpty();
        comment.CreatedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMinutes(1));
    }

    [Fact(DisplayName = "Search comments")]
    public async Task SearchComments()
    {
        /*
        ┌───────────────────────┬───────────────────────┐
        │  COMMENT              │  AUTHOR ID            │
        ├───────────────────────┼───────────────────────┤
        │  comment1             │  authorId1            │
        │   • comment11         │  authorId1            │
        │      • comment111     │  authorId1            │
        │      • comment112     │  authorId2            │
        │   • comment12         │  authorId1            │
        │      • comment121     │  authorId4            │
        │   • comment13         │  authorId3            │
        │   • comment14         │  authorId4            │
        │  comment2             │  authorId1            │
        │   • comment21         │  authorId3            │
        │  comment3             │  authorId4            │
        └───────────────────────┴───────────────────────┘
        */

        // Arrange
        await using var scope = _services.CreateAsyncScope();

        var discussions = scope.ServiceProvider.GetRequiredService<IDiscussionsRepository>();
        var comments = scope.ServiceProvider.GetRequiredService<ICommentRepository>();

        var authorId1 = Guid.NewGuid();
        var authorId2 = Guid.NewGuid();
        var authorId3 = Guid.NewGuid();
        var authorId4 = Guid.NewGuid();

        var discussion = await discussions.CreateAsync(
            authorId: authorId1,
            title: "Discussion",
            description: "Test discussion.");

        var comment1 = await comments.CreateAsync(authorId1, discussion.Id, "Comment 1");
        var comment2 = await comments.CreateAsync(authorId1, discussion.Id, "Comment 2");
        var comment3 = await comments.CreateAsync(authorId4, discussion.Id, "Comment 3");

        var comment11 = await comments.ReplyToCommentAsync(authorId1, comment1.Id, "Comment 11");
        var comment12 = await comments.ReplyToCommentAsync(authorId1, comment1.Id, "Comment 12");
        var comment13 = await comments.ReplyToCommentAsync(authorId3, comment1.Id, "Comment 13");
        var comment14 = await comments.ReplyToCommentAsync(authorId4, comment1.Id, "Comment 14");
        var comment21 = await comments.ReplyToCommentAsync(authorId3, comment2.Id, "Comment 21");

        var comment111 = await comments.ReplyToCommentAsync(authorId1, comment1.Id, "Comment 111");
        var comment112 = await comments.ReplyToCommentAsync(authorId2, comment1.Id, "Comment 112");
        var comment121 = await comments.ReplyToCommentAsync(authorId4, comment1.Id, "Comment 121");

        comment1 = await comments.GetByIdAsync(comment1.Id);
        comment2 = await comments.GetByIdAsync(comment2.Id);
        comment3 = await comments.GetByIdAsync(comment3.Id);

        // Act
        var page = await comments.SearchAsync(
            discussionId: discussion.Id,
            authorId: null,
            offset: 0,
            count: 2);

        // Assert
        page.TotalCount.Should().Be(3);
        page.Items.Should().HaveCount(2);
        page.Items.Should().BeEquivalentTo([comment1, comment2]);
        page.Items.First().FirstRepliesAuthorIds.Should().BeEquivalentTo([authorId4, authorId2, authorId1]);
    }

    [Fact(DisplayName = "Get comment with unknown ID")]
    public async Task GetComment_WithUnknownId_ThrowsNotFoundException()
    {
        // Arrange
        await using var scope = _services.CreateAsyncScope();
        var comments = scope.ServiceProvider.GetRequiredService<ICommentRepository>();

        // Act
        Func<Task> getComment = async () => await comments.GetByIdAsync(Guid.NewGuid());

        // Assert
        await getComment.Should().ThrowAsync<NotFoundException>();
    }

    [Fact(DisplayName = "Get replies to comment")]
    public async Task GetRepliesToComment()
    {
        // Arrange
        await using var scope = _services.CreateAsyncScope();
        var discussions = scope.ServiceProvider.GetRequiredService<IDiscussionsRepository>();
        var comments = scope.ServiceProvider.GetRequiredService<ICommentsService>();

        var authorId = Guid.NewGuid();
        var discussion = await discussions.CreateAsync(authorId, "Discussion", "Test discussion.");
        var comment = await comments.CreateAsync(authorId, discussion.Id, "Comment content");
        var reply = await comments.ReplyToCommentAsync(authorId, comment.Id, "Reply content");

        // Act
        var page = await comments.GetRepliesToCommentAsync(comment.Id, offset: 0, count: 1);

        // Assert
        page.Should().BeEquivalentTo(new Page<CommentDto>
        {
            TotalCount = 1,
            Items = [reply.Adapt<CommentDto>()]
        });
    }

    [Fact(DisplayName = "Create comment with unknown author and discussion")]
    public async Task CreateComment_WithUnknownAuthorAndDiscussion_ThrowsNotFoundException()
    {
        // Arrange
        await using var scope = _services.CreateAsyncScope();
        var comments = scope.ServiceProvider.GetRequiredService<ICommentRepository>();

        // Act
        Func<Task> createComment = async () =>
            await comments.CreateAsync(Guid.NewGuid(), Guid.NewGuid(), "Comment content");

        // Assert
        await createComment.Should().ThrowAsync<NotFoundException>();
    }
}