using DashiToon.Api.Domain.Entities;
using FluentAssertions;

namespace Domain.UnitTests;

public class ReviewTests
{
    [Fact]
    public void CreateReviewShouldSuccess()
    {
        // Arrange
        string? userId = Guid.NewGuid().ToString();
        int seriesId = 1;
        bool isRecommended = true;
        string? content = new('*', 1024);

        // Act
        Review? review = Review.Create(
            userId,
            seriesId,
            isRecommended,
            content
        );

        // Assert
        review.SeriesId.Should().Be(seriesId);
        review.UserId.Should().Be(userId);
        review.IsRecommended.Should().BeTrue();
        review.Content.Should().Be(content);
        review.Likes.Should().Be(0);
        review.Dislikes.Should().Be(0);
        review.Timestamp.Should().BeWithin(TimeSpan.FromMilliseconds(100));
    }

    [Theory]
    [MemberData(nameof(CreateReviewShouldRequireValidContentTestCases))]
    public void CreateReviewShouldRequireValidContent(string content)
    {
        // Arrange
        string? userId = Guid.NewGuid().ToString();
        int chapterId = 1;

        // Act
        FluentActions.Invoking(() => Review.Create(
                userId,
                chapterId,
                true,
                content
            ))
            .Should().Throw<ArgumentException>();
    }

    public static IEnumerable<object[]> CreateReviewShouldRequireValidContentTestCases()
    {
        return
        [
            [new string('*', 99)],
            [new string('*', 8193)]
        ];
    }

    [Fact]
    public void UpdateReviewShouldSuccess()
    {
        // Arrange
        string? userId = Guid.NewGuid().ToString();
        int seriesId = 1;
        bool isRecommended = true;
        string? content = new('*', 1024);

        Review? review = Review.Create(
            userId,
            seriesId,
            isRecommended,
            content
        );

        string? updatedContent = new('*', 300);
        // Act

        review.Update(updatedContent, false);

        // Assert
        review.Content.Should().Be(updatedContent);
        review.IsRecommended.Should().BeFalse();
    }

    [Fact]
    public void RateReviewShouldSuccess()
    {
        // Arrange
        string? userId = Guid.NewGuid().ToString();
        int chapterId = 1;
        string? content = new('*', 1024);

        Review? review = Review.Create(
            userId,
            chapterId,
            true,
            content
        );

        string? rateUserId = Guid.NewGuid().ToString();
        bool liked = true;

        // Act
        review.Rate(rateUserId, liked);

        // Assert
        review.ReviewRates.Should().HaveCount(1);
        review.Likes.Should().Be(1);
        review.Dislikes.Should().Be(0);
    }

    [Fact]
    public void RateReviewShouldNotAllowedRatingFromSameUser()
    {
        // Arrange
        string? userId = Guid.NewGuid().ToString();
        int chapterId = 1;
        string? content = new('*', 1024);

        Review? review = Review.Create(
            userId,
            chapterId,
            true,
            content
        );

        string? rateUserId = Guid.NewGuid().ToString();
        bool liked = true;

        review.Rate(rateUserId, liked);

        // Act
        review.Rate(rateUserId, liked);

        // Assert
        review.ReviewRates.Should().HaveCount(1);
        review.Likes.Should().Be(1);
        review.Dislikes.Should().Be(0);
    }
}
