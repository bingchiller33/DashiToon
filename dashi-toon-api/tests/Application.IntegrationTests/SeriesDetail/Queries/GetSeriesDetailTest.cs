using DashiToon.Api.Application.AuthorStudio.Series.Commands.CreateSeries;
using DashiToon.Api.Application.AuthorStudio.Series.Commands.UpdateSeries;
using DashiToon.Api.Application.SeriesDetail.Queries.GetSeriesDetail;
using DashiToon.Api.Domain.Enums;

namespace Application.IntegrationTests.SeriesDetail.Queries;

using static Testing;

[Collection("Serialize")]
public class GetSeriesDetailTest : BaseIntegrationTest
{
    public GetSeriesDetailTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task GetSeriesDetailShouldReturnCorrectDetail()
    {
        // Arrange
        await RunAsDefaultUserAsync();

        int seriesId = await SendAsync(new CreateSeriesCommand(
            "TestTitle",
            "TestSynopsis",
            "TestThumbnail",
            SeriesType.Comic,
            [1, 2, 3],
            [
                new ContentQuestionnaire(ContentCategory.Violent, 1),
                new ContentQuestionnaire(ContentCategory.Nudity, 2),
                new ContentQuestionnaire(ContentCategory.Profanity, 1),
                new ContentQuestionnaire(ContentCategory.Alcohol, 3),
                new ContentQuestionnaire(ContentCategory.Sensitive, 1),
                new ContentQuestionnaire(ContentCategory.Sexual, 2)
            ],
            ["Alt 1"],
            ["Author1"],
            null));

        await SendAsync(new UpdateSeriesCommand(
            seriesId,
            "TestTitle",
            "TestSynopsis",
            "TestThumbnail",
            SeriesStatus.Ongoing,
            [1, 2, 3],
            [
                new ContentQuestionnaire(ContentCategory.Violent, 1),
                new ContentQuestionnaire(ContentCategory.Nudity, 1),
                new ContentQuestionnaire(ContentCategory.Profanity, 1),
                new ContentQuestionnaire(ContentCategory.Alcohol, 1),
                new ContentQuestionnaire(ContentCategory.Sensitive, 1),
                new ContentQuestionnaire(ContentCategory.Sexual, 1)
            ],
            ["Alt 1"],
            ["Author1"],
            null));

        // Act
        SeriesDetailVm seriesDetail = await SendAsync(new GetSeriesDetailQuery(seriesId));

        // Assert
        seriesDetail.Should().NotBeNull();
        seriesDetail.Id.Should().Be(seriesId);
        seriesDetail.AlternativeTitles.Should().BeEquivalentTo("Alt 1");
        seriesDetail.Title.Should().Be("TestTitle");
        seriesDetail.Status.Should().Be(SeriesStatus.Ongoing);
        seriesDetail.Author.Should().Be("Author1");
        seriesDetail.Synopsis.Should().Be("TestSynopsis");
        seriesDetail.Thumbnail.Should().Be("thumbnails/TestThumbnail");
        seriesDetail.Type.Should().Be(SeriesType.Comic);
        seriesDetail.Genres.Should().HaveCount(3);
        seriesDetail.ContentRating.Should().Be(ContentRating.Teen);
    }

    [Fact]
    public async Task GetSeriesDetailShouldRequireExistingSeries()
    {
        // Act
        await RunAsDefaultUserAsync();

        await FluentActions.Invoking(async () => await SendAsync(new GetSeriesDetailQuery(1)))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Theory]
    [InlineData(SeriesStatus.Hiatus)]
    [InlineData(SeriesStatus.Trashed)]
    [InlineData(SeriesStatus.Draft)]
    public async Task GetSeriesDetailShouldRequireValidSeriesStatus(SeriesStatus status)
    {
        // Arrange
        await RunAsDefaultUserAsync();

        int seriesId = await SendAsync(new CreateSeriesCommand(
            "TestTitle",
            "TestSynopsis",
            "TestThumbnail",
            SeriesType.Comic,
            [1, 2, 3],
            [
                new ContentQuestionnaire(ContentCategory.Violent, 1),
                new ContentQuestionnaire(ContentCategory.Nudity, 2),
                new ContentQuestionnaire(ContentCategory.Profanity, 1),
                new ContentQuestionnaire(ContentCategory.Alcohol, 3),
                new ContentQuestionnaire(ContentCategory.Sensitive, 1),
                new ContentQuestionnaire(ContentCategory.Sexual, 2)
            ],
            [],
            [],
            null));

        await SendAsync(new UpdateSeriesCommand(
            seriesId,
            "TestTitle",
            "TestSynopsis",
            "TestThumbnail",
            status,
            [1, 2, 3],
            [
                new ContentQuestionnaire(ContentCategory.Violent, 1),
                new ContentQuestionnaire(ContentCategory.Nudity, 2),
                new ContentQuestionnaire(ContentCategory.Profanity, 1),
                new ContentQuestionnaire(ContentCategory.Alcohol, 3),
                new ContentQuestionnaire(ContentCategory.Sensitive, 1),
                new ContentQuestionnaire(ContentCategory.Sexual, 2)
            ],
            [],
            [],
            null));

        // Act
        await FluentActions.Invoking(async () => await SendAsync(new GetSeriesDetailQuery(seriesId)))
            .Should().ThrowAsync<NotFoundException>();
    }
}
