using DashiToon.Api.Application.AuthorStudio.Series.Commands.CreateSeries;
using DashiToon.Api.Application.AuthorStudio.Series.Queries.GetSeries;
using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Domain.Enums;

namespace Application.IntegrationTests.AuthorStudio.Series.Queries;

using static Testing;

[Collection("Serialize")]
public class GetSeriesTest : BaseIntegrationTest
{
    public GetSeriesTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task GetSeriesShouldReturnSeriesDetail()
    {
        // Arrange
        string userId = await RunAsDefaultUserAsync();

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
            ["Alt 1", "Alt 2"],
            ["Auth 1"],
            null));

        // Act
        SeriesDetailVm series = await SendAsync(new GetSeriesQuery(seriesId));

        // Assert
        IApplicationDbContext context = GetContext();

        List<string> genres = context.Genres
            .Where(g => new[] { 1, 2, 3 }
                .Contains(g.Id))
            .Select(x => x.Name).ToList();

        series.Should().NotBeNull();
        series.Id.Should().Be(seriesId);
        series.Title.Should().Be("TestTitle");
        series.AlternativeTitles.Should().BeEquivalentTo("Alt 1", "Alt 2");
        series.Authors.Should().BeEquivalentTo(["Auth 1"]);
        series.StartTime.Should().Be(string.Empty);
        series.Status.Should().Be(SeriesStatus.Draft);
        series.Synopsis.Should().Be("TestSynopsis");
        series.Thumbnail.Should().Be("TestThumbnail");
        series.Type.Should().Be(SeriesType.Comic);
        series.ContentRating.Should().Be(ContentRating.Mature);
        series.Genres.Should().BeEquivalentTo(genres);
        series.CategoryRatings.Should().BeEquivalentTo(new CategoryRatingVm[]
        {
            new(ContentCategory.Violent.ToString(), 1), new(ContentCategory.Nudity.ToString(), 2),
            new(ContentCategory.Profanity.ToString(), 1), new(ContentCategory.Alcohol.ToString(), 3),
            new(ContentCategory.Sensitive.ToString(), 1), new(ContentCategory.Sexual.ToString(), 2)
        });
    }

    [Fact]
    public async Task GetSeriesShouldRequireExistingSeries()
    {
        // Act & Assert
        await RunAsDefaultUserAsync();

        await FluentActions.Invoking(() => SendAsync(new GetSeriesQuery(1)))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetSeriesShouldRequireValidUser()
    {
        // Arrange
        string userId = await RunAsDefaultUserAsync();

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
            ["Alt 1", "Alt 2"],
            ["Auth 1"],
            null));

        await RunAsUserAsync("AnotherGuy", "Itsapassword1!", []);

        // Act
        await FluentActions.Invoking(() => SendAsync(new GetSeriesQuery(seriesId)))
            .Should().ThrowAsync<ForbiddenAccessException>();
    }
}
