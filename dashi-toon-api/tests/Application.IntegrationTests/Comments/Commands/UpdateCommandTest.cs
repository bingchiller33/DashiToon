using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.CreateNovelChapter;
using DashiToon.Api.Application.AuthorStudio.Series.Commands.CreateSeries;
using DashiToon.Api.Application.AuthorStudio.Volumes.Commands.CreateVolume;
using DashiToon.Api.Application.Comments.Commands.UpdateComment;
using DashiToon.Api.Application.Comments.Commands.WriteComment;
using DashiToon.Api.Application.Comments.Models;
using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Domain.Enums;

namespace Application.IntegrationTests.Comments.Commands;

using static Testing;

[Collection("Serialize")]
public class UpdateCommandTest : BaseIntegrationTest
{
    public UpdateCommandTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task UpdateCommentShouldUpdateComment()
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
        CommentVm? updatedComment = await SendAsync(new UpdateCommentCommand(chapterId, comment.Id, "UpdatedComment"));

        // Assert
        updatedComment.Content.Should().Be("UpdatedComment");
        updatedComment.IsEdited.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateCommentShouldRequireExistingCommentAndChapter()
    {
        // Arrange
        await RunAsDefaultUserAsync();

        // Act & Assert
        await FluentActions.Invoking(() => SendAsync(new UpdateCommentCommand(1, Guid.Empty, "UpdatedComment")))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateCommentShouldOnlyAllowAuthor()
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

        await RunAsUserAsync("User1", "Bruh@1311", []);

        // Act & Assert
        await FluentActions.Invoking(() => SendAsync(new UpdateCommentCommand(chapterId, comment.Id, "UpdatedComment")))
            .Should().ThrowAsync<ForbiddenAccessException>();
    }
}
