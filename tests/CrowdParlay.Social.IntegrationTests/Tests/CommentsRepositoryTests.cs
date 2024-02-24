// ReSharper disable UnusedVariable
// ReSharper disable RedundantAssignment

namespace CrowdParlay.Social.IntegrationTests.Tests;

public class CommentsRepositoryTests(WebApplicationContext context) : IClassFixture<WebApplicationContext>
{
    private readonly IServiceProvider _services = context.Services;

    [Fact(DisplayName = "Create comment")]
    public async Task CreateComment()
    {
        // Arrange
        await using var scope = _services.CreateAsyncScope();

        var authors = scope.ServiceProvider.GetRequiredService<IAuthorRepository>();
        var discussions = scope.ServiceProvider.GetRequiredService<IDiscussionRepository>();
        var comments = scope.ServiceProvider.GetRequiredService<ICommentRepository>();

        var author = await authors.CreateAsync(Guid.NewGuid(), "author12345", "Author 12345", null);

        var discussion = await discussions.CreateAsync(
            authorId: author.Id,
            title: "Discussion",
            description: "Test discussion.");

        // Act
        var comment = await comments.CreateAsync(author.Id, discussion.Id, "Comment content");

        // Assert
        comment.Author.Should().BeEquivalentTo(author);
        comment.Content.Should().Be("Comment content");
        comment.ReplyCount.Should().Be(0);
        comment.FirstRepliesAuthors.Should().BeEmpty();
        comment.CreatedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMinutes(1));
    }

    [Fact(DisplayName = "Search comments")]
    public async Task SearchComments()
    {
        /*
        ┌───────────────────────┬───────────────────────┐
        │  COMMENT              │  AUTHOR               │
        ├───────────────────────┼───────────────────────┤
        │  comment1             │  author1              │
        │   • comment11         │  author1              │
        │      • comment111     │  author1              │
        │      • comment112     │  author2              │
        │   • comment12         │  author1              │
        │      • comment121     │  author4              │
        │   • comment13         │  author3              │
        │   • comment14         │  author4              │
        │  comment2             │  author1              │
        │   • comment21         │  author3              │
        │  comment3             │  author4              │
        └───────────────────────┴───────────────────────┘
        */

        // Arrange
        await using var scope = _services.CreateAsyncScope();

        var authors = scope.ServiceProvider.GetRequiredService<IAuthorRepository>();
        var discussions = scope.ServiceProvider.GetRequiredService<IDiscussionRepository>();
        var comments = scope.ServiceProvider.GetRequiredService<ICommentRepository>();

        var author1 = await authors.CreateAsync(Guid.NewGuid(), "author_1", "Author 1", null);
        var author2 = await authors.CreateAsync(Guid.NewGuid(), "author_2", "Author 2", null);
        var author3 = await authors.CreateAsync(Guid.NewGuid(), "author_3", "Author 3", null);
        var author4 = await authors.CreateAsync(Guid.NewGuid(), "author_4", "Author 4", null);

        var discussion = await discussions.CreateAsync(
            authorId: author1.Id,
            title: "Discussion",
            description: "Test discussion.");

        var comment1 = await comments.CreateAsync(author1.Id, discussion.Id, "Comment 1");
        var comment2 = await comments.CreateAsync(author1.Id, discussion.Id, "Comment 2");
        var comment3 = await comments.CreateAsync(author4.Id, discussion.Id, "Comment 3");

        var comment11 = await comments.ReplyToCommentAsync(author1.Id, comment1.Id, "Comment 11");
        var comment12 = await comments.ReplyToCommentAsync(author1.Id, comment1.Id, "Comment 12");
        var comment13 = await comments.ReplyToCommentAsync(author3.Id, comment1.Id, "Comment 13");
        var comment14 = await comments.ReplyToCommentAsync(author4.Id, comment1.Id, "Comment 14");
        var comment21 = await comments.ReplyToCommentAsync(author3.Id, comment2.Id, "Comment 21");

        var comment111 = await comments.ReplyToCommentAsync(author1.Id, comment1.Id, "Comment 111");
        var comment112 = await comments.ReplyToCommentAsync(author2.Id, comment1.Id, "Comment 112");
        var comment121 = await comments.ReplyToCommentAsync(author4.Id, comment1.Id, "Comment 121");

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
        page.Items.Should().BeEquivalentTo(new[] { comment1, comment2 });
        page.Items.First().FirstRepliesAuthors.Should().BeEquivalentTo(new[] { author4, author2, author1 });
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