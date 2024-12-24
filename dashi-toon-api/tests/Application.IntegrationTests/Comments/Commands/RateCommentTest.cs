using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.CreateNovelChapter;
using DashiToon.Api.Application.AuthorStudio.Series.Commands.CreateSeries;
using DashiToon.Api.Application.AuthorStudio.Volumes.Commands.CreateVolume;
using DashiToon.Api.Application.Comments.Commands.RateComment;
using DashiToon.Api.Application.Comments.Commands.WriteComment;
using DashiToon.Api.Application.Comments.Models;
using DashiToon.Api.Domain.Enums;

namespace Application.IntegrationTests.Comments.Commands;

using static Testing;

[Collection("Serialize")]
public class RateCommentTest : BaseIntegrationTest
{
    public RateCommentTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task RateCommentShouldSucceed()
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

        CommentVm? comment = await SendAsync(new WriteCommentCommand(chapterId, "TestComment"));

        // Act
        CommentVm? ratedComment = await SendAsync(new RateCommentCommand(chapterId, comment.Id, true));

        // Assert
        ratedComment.Likes.Should().Be(1);
        ratedComment.IsEdited.Should().BeFalse();
    }

    [Fact]
    public async Task RateCommentShouldRequireExistingCommentAndChapter()
    {
        // Arrange
        string userId = await RunAsDefaultUserAsync();

        // Act & Assert
        await FluentActions.Invoking(() => SendAsync(new RateCommentCommand(1, Guid.Empty, true)))
            .Should().ThrowAsync<NotFoundException>();
    }
}
