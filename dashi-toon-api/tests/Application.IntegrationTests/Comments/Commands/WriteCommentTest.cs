using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.CreateNovelChapter;
using DashiToon.Api.Application.AuthorStudio.Series.Commands.CreateSeries;
using DashiToon.Api.Application.AuthorStudio.Volumes.Commands.CreateVolume;
using DashiToon.Api.Application.Comments.Commands.WriteComment;
using DashiToon.Api.Application.Comments.Models;
using DashiToon.Api.Domain.Enums;

namespace Application.IntegrationTests.Comments.Commands;

using static Testing;

[Collection("Serialize")]
public class WriteCommentTest : BaseIntegrationTest
{
    public WriteCommentTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task WriteCommentShouldSucceed()
    {
        // Arrange
        string userId = await RunAsDefaultUserAsync();

        int seriesId = await SendAsync(new CreateSeriesCommand(
            "TestTitle",
            "TestSynopsis",
            "TestThumbnail",
            SeriesType.Novel,
            [1, 2, 3],
            [
                new ContentQuestionnaire(ContentCategory.Violent, 1),
                new ContentQuestionnaire(ContentCategory.Nudity, 1),
                new ContentQuestionnaire(ContentCategory.Profanity, 1),
                new ContentQuestionnaire(ContentCategory.Alcohol, 1),
                new ContentQuestionnaire(ContentCategory.Sensitive, 1),
                new ContentQuestionnaire(ContentCategory.Sexual, 1)
            ],
            [],
            [],
            null));

        int volumeId = await SendAsync(new CreateVolumeCommand(seriesId, "TestName", "TestIntroduction"));

        int chapterId = await SendAsync(new CreateNovelChapterCommand(
            seriesId,
            volumeId,
            "TestTitle",
            "TestThumbnail",
            "<p>My life be like ooooh ahhhhhh</p>",
            "TestNote"
        ));

        // Act
        CommentVm? comment = await SendAsync(new WriteCommentCommand(chapterId, "TestComment"));

        // Assert
        comment.Content.Should().Be("TestComment");
        comment.Likes.Should().Be(0);
        comment.Dislikes.Should().Be(0);
        comment.RepliesCount.Should().Be(0);
        comment.UserId.Should().Be(userId);
        comment.Username.Should().Be(GetUserName());
        comment.IsEdited.Should().BeFalse();
    }

    [Fact]
    public async Task WriteCommentShouldRequireExistingChapter()
    {
        // Arrange
        string userId = await RunAsDefaultUserAsync();

        // Act & Assert
        await FluentActions.Invoking(() => SendAsync(new WriteCommentCommand(1, "TestComment")))
            .Should().ThrowAsync<NotFoundException>();
    }
}
