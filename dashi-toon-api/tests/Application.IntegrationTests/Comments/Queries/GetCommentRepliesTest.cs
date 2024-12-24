using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.CreateNovelChapter;
using DashiToon.Api.Application.AuthorStudio.Series.Commands.CreateSeries;
using DashiToon.Api.Application.AuthorStudio.Volumes.Commands.CreateVolume;
using DashiToon.Api.Application.Comments.Commands.RateComment;
using DashiToon.Api.Application.Comments.Commands.ReplyComment;
using DashiToon.Api.Application.Comments.Commands.WriteComment;
using DashiToon.Api.Application.Comments.Models;
using DashiToon.Api.Application.Comments.Queries.GetCommentReplies;
using DashiToon.Api.Domain.Enums;

namespace Application.IntegrationTests.Comments.Queries;

using static Testing;

[Collection("Serialize")]
public class GetCommentRepliesTest : BaseIntegrationTest
{
    public GetCommentRepliesTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task GetCommentRepliesShouldReturnAllChildComments()
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

        CommentVm? reply1 = await SendAsync(new ReplyCommentCommand(chapterId, comment1.Id, "Reply1"));

        string? userId2 = GetUserId();
        string? username2 = GetUserName();

        CommentVm? reply2 = await SendAsync(new ReplyCommentCommand(chapterId, reply1.Id, "Reply2"));

        // Act
        List<ReplyVm>? comments = await SendAsync(new GetCommentRepliesQuery(chapterId, comment1.Id));

        // Assert
        comments.Should().HaveCount(2);

        comments.First().Id.Should().Be(reply1.Id);
        comments.First().Content.Should().Be($"@{username1} Reply1");
        comments.First().Likes.Should().Be(0);
        comments.First().Dislikes.Should().Be(0);
        comments.First().UserId.Should().Be(userId2);
        comments.First().Username.Should().Be(username2);
        comments.First().IsEdited.Should().BeFalse();

        comments.Last().Id.Should().Be(reply2.Id);
        comments.Last().Content.Should().Be($"@{username2} Reply2");
        comments.Last().Likes.Should().Be(0);
        comments.Last().Dislikes.Should().Be(0);
        comments.Last().UserId.Should().Be(userId2);
        comments.Last().Username.Should().Be(username2);
        comments.Last().IsEdited.Should().BeFalse();
    }
}
