using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.CreateNovelChapter;
using DashiToon.Api.Application.AuthorStudio.Series.Commands.CreateSeries;
using DashiToon.Api.Application.AuthorStudio.Volumes.Commands.CreateVolume;
using DashiToon.Api.Application.Comments.Commands.RateComment;
using DashiToon.Api.Application.Comments.Commands.ReplyComment;
using DashiToon.Api.Application.Comments.Commands.WriteComment;
using DashiToon.Api.Application.Comments.Models;
using DashiToon.Api.Application.Comments.Queries.GetComments;
using DashiToon.Api.Application.Common.Models;
using DashiToon.Api.Domain.Enums;

namespace Application.IntegrationTests.Comments.Queries;

using static Testing;

[Collection("Serialize")]
public class GetCommentsTest : BaseIntegrationTest
{
    public GetCommentsTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task GetCommentsShouldReturnAllHeadComments()
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

        string? userId1 = userId;
        string? username1 = GetUserName();
        CommentVm? comment1 = await SendAsync(new WriteCommentCommand(chapterId, "TestComment1"));
        await SendAsync(new RateCommentCommand(chapterId, comment1.Id, true));

        await RunAsUserAsync("TestUser2", "Bruh@1311", []);

        string? userId2 = GetUserId();
        string? username2 = GetUserName();

        await SendAsync(new ReplyCommentCommand(chapterId, comment1.Id, "Reply1"));
        CommentVm? comment2 = await SendAsync(new WriteCommentCommand(chapterId, "TestComment2"));

        // Act
        PaginatedList<CommentVm>? comments = await SendAsync(new GetChapterCommentsQuery(chapterId));

        // Assert
        comments.Items.Should().HaveCount(2);

        comments.Items.First().Id.Should().Be(comment1.Id);
        comments.Items.First().Content.Should().Be("TestComment1");
        comments.Items.First().Likes.Should().Be(1);
        comments.Items.First().Dislikes.Should().Be(0);
        comments.Items.First().UserId.Should().Be(userId1);
        comments.Items.First().Username.Should().Be(username1);
        comments.Items.First().IsEdited.Should().BeFalse();

        comments.Items.Last().Id.Should().Be(comment2.Id);
        comments.Items.Last().Content.Should().Be("TestComment2");
        comments.Items.Last().Likes.Should().Be(0);
        comments.Items.Last().Dislikes.Should().Be(0);
        comments.Items.Last().UserId.Should().Be(userId2);
        comments.Items.Last().Username.Should().Be(username2);
        comments.Items.Last().IsEdited.Should().BeFalse();
    }
}
