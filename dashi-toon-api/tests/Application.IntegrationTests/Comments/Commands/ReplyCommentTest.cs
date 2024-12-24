using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.CreateNovelChapter;
using DashiToon.Api.Application.AuthorStudio.Series.Commands.CreateSeries;
using DashiToon.Api.Application.AuthorStudio.Volumes.Commands.CreateVolume;
using DashiToon.Api.Application.Comments.Commands.ReplyComment;
using DashiToon.Api.Application.Comments.Commands.WriteComment;
using DashiToon.Api.Application.Comments.Models;
using DashiToon.Api.Domain.Enums;

namespace Application.IntegrationTests.Comments.Commands;

using static Testing;

[Collection("Serialize")]
public class ReplyCommentTest : BaseIntegrationTest
{
    public ReplyCommentTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task ReplyCommentShouldSucceed()
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
        CommentVm? reply = await SendAsync(new ReplyCommentCommand(chapterId, comment.Id, "ReplyComment"));

        // Assert
        reply.Content.Should().Be("ReplyComment");
        reply.Likes.Should().Be(0);
        reply.Dislikes.Should().Be(0);
        reply.RepliesCount.Should().Be(0);
        reply.UserId.Should().Be(userId);
        reply.Username.Should().Be(GetUserName());
        reply.IsEdited.Should().BeFalse();
    }

    [Fact]
    public async Task ReplyCommentShouldRequireExistingCommentAndChapter()
    {
        // Arrange
        string userId = await RunAsDefaultUserAsync();

        // Act & Assert
        await FluentActions.Invoking(() => SendAsync(new ReplyCommentCommand(1, Guid.Empty, "ReplyComment")))
            .Should().ThrowAsync<NotFoundException>();
    }
}
