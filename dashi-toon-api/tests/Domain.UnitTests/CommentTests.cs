using DashiToon.Api.Domain.Entities;
using FluentAssertions;

namespace Domain.UnitTests;

public class CommentTests
{
    [Fact]
    public void CreateCommentShouldSuccess()
    {
        // Arrange
        string? userId = Guid.NewGuid().ToString();
        int chapterId = 1;
        string? content = new('*', 1024);

        // Act
        Comment? comment = Comment.Create(
            userId,
            chapterId,
            content
        );

        // Assert
        comment.ChapterId.Should().Be(chapterId);
        comment.UserId.Should().Be(userId);
        comment.Content.Should().Be(content);
        comment.Likes.Should().Be(0);
        comment.Dislikes.Should().Be(0);
        comment.Timestamp.Should().BeWithin(TimeSpan.FromMilliseconds(100));
        comment.ParentCommentId.Should().BeNull();
        comment.ParentComment.Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(CreateCommentShouldRequireValidContentTestCases))]
    public void CreateCommentShouldRequireValidContent(string content)
    {
        // Arrange
        string? userId = Guid.NewGuid().ToString();
        int chapterId = 1;

        // Act
        FluentActions.Invoking(() => Comment.Create(
                userId,
                chapterId,
                content
            ))
            .Should().Throw<ArgumentException>();
    }

    public static IEnumerable<object[]> CreateCommentShouldRequireValidContentTestCases()
    {
        return
        [
            [new string('*', 2049)],
            [string.Empty]
        ];
    }

    [Fact]
    public void UpdateCommentShouldSuccess()
    {
        // Arrange
        string? userId = Guid.NewGuid().ToString();
        int chapterId = 1;
        string? content = new('*', 1024);

        Comment? comment = Comment.Create(
            userId,
            chapterId,
            content
        );

        string? updatedContent = new('*', 300);
        // Act

        comment.Update(updatedContent);

        // Assert
        comment.Content.Should().Be(updatedContent);
    }

    [Fact]
    public void ReplyCommentShouldSuccess()
    {
        // Arrange
        string? userId = Guid.NewGuid().ToString();
        int chapterId = 1;
        string? content = new('*', 1024);

        Comment? comment = Comment.Create(
            userId,
            chapterId,
            content
        );

        string? replyUser = Guid.NewGuid().ToString();
        string? replyContent = new('*', 100);
        // Act
        comment.ReplyComment(replyUser, replyContent);

        // Assert
        comment.Replies.Should().HaveCount(1);
        comment.Replies[0].UserId.Should().Be(replyUser);
        comment.Replies[0].Content.Should().Be(replyContent);
        comment.Replies[0].ChapterId.Should().Be(chapterId);
        comment.Replies[0].Likes.Should().Be(0);
        comment.Replies[0].Dislikes.Should().Be(0);
    }

    [Fact]
    public void RateCommentShouldSuccess()
    {
        // Arrange
        string? userId = Guid.NewGuid().ToString();
        int chapterId = 1;
        string? content = new('*', 1024);

        Comment? comment = Comment.Create(
            userId,
            chapterId,
            content
        );

        string? rateUserId = Guid.NewGuid().ToString();
        bool liked = true;

        // Act
        comment.Rate(rateUserId, liked);

        // Assert
        comment.CommentRates.Should().HaveCount(1);
        comment.Likes.Should().Be(1);
        comment.Dislikes.Should().Be(0);
    }

    [Fact]
    public void RateCommentShouldNotAllowedRatingFromSameUser()
    {
        // Arrange
        string? userId = Guid.NewGuid().ToString();
        int chapterId = 1;
        string? content = new('*', 1024);

        Comment? comment = Comment.Create(
            userId,
            chapterId,
            content
        );

        string? rateUserId = Guid.NewGuid().ToString();
        bool liked = true;

        comment.Rate(rateUserId, liked);

        // Act
        comment.Rate(rateUserId, liked);

        // Assert
        comment.CommentRates.Should().HaveCount(1);
        comment.Likes.Should().Be(1);
        comment.Dislikes.Should().Be(0);
    }
}
