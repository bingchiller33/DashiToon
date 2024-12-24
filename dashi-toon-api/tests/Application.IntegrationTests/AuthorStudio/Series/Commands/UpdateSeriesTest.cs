using DashiToon.Api.Application.AuthorStudio.Series.Commands.CreateSeries;
using DashiToon.Api.Application.AuthorStudio.Series.Commands.UpdateSeries;
using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.IntegrationTests.AuthorStudio.Series.Commands;

using static Testing;

[Collection("Serialize")]
public class UpdateSeriesTest : BaseIntegrationTest
{
    public UpdateSeriesTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    public static IEnumerable<object[]> UpdateSeriesShouldRequireValidAttributesTestCases =>
    [
        [
            // Invalid rating option
            "TestTitle",
            "TestSynopsis",
            "TestThumbnail",
            SeriesStatus.Completed,
            new[] { 1, 2, 3 },
            new[]
            {
                new ContentQuestionnaire(ContentCategory.Violent, -1),
                new ContentQuestionnaire(ContentCategory.Nudity, 3),
                new ContentQuestionnaire(ContentCategory.Profanity, 1),
                new ContentQuestionnaire(ContentCategory.Alcohol, -3),
                new ContentQuestionnaire(ContentCategory.Sensitive, 1),
                new ContentQuestionnaire(ContentCategory.Sexual, -2)
            }
        ],
        [
            // Invalid rating option
            "TestTitle",
            "TestSynopsis",
            "TestThumbnail",
            SeriesStatus.Completed,
            new[] { 1, 2, 3 },
            new[]
            {
                new ContentQuestionnaire(ContentCategory.Violent, 0),
                new ContentQuestionnaire(ContentCategory.Nudity, 2),
                new ContentQuestionnaire(ContentCategory.Profanity, 1),
                new ContentQuestionnaire(ContentCategory.Alcohol, 2),
                new ContentQuestionnaire(ContentCategory.Sensitive, 3),
                new ContentQuestionnaire(ContentCategory.Sexual, 1),
                new ContentQuestionnaire(ContentCategory.Sexual, 2)
            }
        ],
        [
            // Invalid rating option
            "TestTitle",
            "TestSynopsis",
            "TestThumbnail",
            SeriesStatus.Completed,
            new[] { 1, 2, 3 },
            new[]
            {
                new ContentQuestionnaire(ContentCategory.Violent, 0),
                new ContentQuestionnaire(ContentCategory.Nudity, 2),
                new ContentQuestionnaire(ContentCategory.Profanity, 1),
                new ContentQuestionnaire(ContentCategory.Alcohol, 2)
            }
        ],
        [
            // Invalid Title
            string.Empty,
            "TestSynopsis",
            "TestThumbnail",
            SeriesStatus.Completed,
            new[] { 1, 2, 3 },
            new[]
            {
                new ContentQuestionnaire(ContentCategory.Violent, 0),
                new ContentQuestionnaire(ContentCategory.Nudity, 0),
                new ContentQuestionnaire(ContentCategory.Profanity, 0),
                new ContentQuestionnaire(ContentCategory.Alcohol, 0),
                new ContentQuestionnaire(ContentCategory.Sensitive, 0),
                new ContentQuestionnaire(ContentCategory.Sexual, 0)
            }
        ],
        [
            // Invalid Title
            new string('a', 256),
            "TestSynopsis",
            "TestThumbnail",
            SeriesStatus.Completed,
            new[] { 1, 2, 3 },
            new[]
            {
                new ContentQuestionnaire(ContentCategory.Violent, 0),
                new ContentQuestionnaire(ContentCategory.Nudity, 0),
                new ContentQuestionnaire(ContentCategory.Profanity, 0),
                new ContentQuestionnaire(ContentCategory.Alcohol, 0),
                new ContentQuestionnaire(ContentCategory.Sensitive, 0),
                new ContentQuestionnaire(ContentCategory.Sexual, 0)
            }
        ],
        [
            // Invalid Synopsis
            "TestTitle",
            string.Empty,
            "TestThumbnail",
            SeriesStatus.Completed,
            new[] { 1, 2, 3 },
            new[]
            {
                new ContentQuestionnaire(ContentCategory.Violent,
                    0),
                new ContentQuestionnaire(ContentCategory.Nudity, 0),
                new ContentQuestionnaire(ContentCategory.Profanity, 0),
                new ContentQuestionnaire(ContentCategory.Alcohol, 0),
                new ContentQuestionnaire(ContentCategory.Sensitive, 0),
                new ContentQuestionnaire(ContentCategory.Sexual, 0)
            }
        ],
        [
            // Invalid Synopsis
            "TestTitle",
            new string('*', 5001),
            "TestThumbnail",
            SeriesStatus.Completed,
            new[] { 1, 2, 3 },
            new[]
            {
                new ContentQuestionnaire(ContentCategory.Violent,
                    0),
                new ContentQuestionnaire(ContentCategory.Nudity, 0),
                new ContentQuestionnaire(ContentCategory.Profanity, 0),
                new ContentQuestionnaire(ContentCategory.Alcohol, 0),
                new ContentQuestionnaire(ContentCategory.Sensitive, 0),
                new ContentQuestionnaire(ContentCategory.Sexual, 0)
            }
        ],
        [
            // Invalid Genres
            "TestTitle",
            "TestSynopsis",
            "TestThumbnail",
            SeriesStatus.Completed,
            Array.Empty<int>(),
            new[]
            {
                new ContentQuestionnaire(ContentCategory.Violent, 1),
                new ContentQuestionnaire(ContentCategory.Nudity, 2),
                new ContentQuestionnaire(ContentCategory.Profanity, 1),
                new ContentQuestionnaire(ContentCategory.Alcohol, 3),
                new ContentQuestionnaire(ContentCategory.Sensitive, 1),
                new ContentQuestionnaire(ContentCategory.Sexual, 2)
            }
        ]
    ];

    [Fact]
    public async Task UpdateSeriesShouldUpdateSuccessfully()
    {
        // Arrange
        string userId = await RunAsDefaultUserAsync();

        int seriesId = await SendAsync(new CreateSeriesCommand(
            "Test Series",
            "Test Series Description",
            "Test Series Thumbnail",
            SeriesType.Comic,
            [1, 2, 3],
            [
                new ContentQuestionnaire(ContentCategory.Violent, 1),
                new ContentQuestionnaire(ContentCategory.Nudity, 2),
                new ContentQuestionnaire(ContentCategory.Profanity, 1),
                new ContentQuestionnaire(ContentCategory.Alcohol, 2),
                new ContentQuestionnaire(ContentCategory.Sensitive, 2),
                new ContentQuestionnaire(ContentCategory.Sexual, 2)
            ],
            [],
            [],
            null
        ));

        UpdateSeriesCommand updateSeriesCommand = new(
            seriesId,
            "Updated Series",
            "Updated Series Description",
            "Updated Series Thumbnail",
            SeriesStatus.Ongoing,
            [1, 2, 3, 4, 5, 1000],
            [
                new ContentQuestionnaire(ContentCategory.Violent, 1),
                new ContentQuestionnaire(ContentCategory.Nudity, 2),
                new ContentQuestionnaire(ContentCategory.Profanity, 1),
                new ContentQuestionnaire(ContentCategory.Alcohol, 2),
                new ContentQuestionnaire(ContentCategory.Sensitive, 3),
                new ContentQuestionnaire(ContentCategory.Sexual, 2)
            ],
            ["Alt 1", "Alt 2"],
            ["Auth 1"],
            null
        );

        // Act
        await SendAsync(updateSeriesCommand);

        // Assert
        IApplicationDbContext context = GetContext();
        DashiToon.Api.Domain.Entities.Series? series = await context.Series
            .Include(s => s.Genres)
            .FirstOrDefaultAsync(s => s.Id == seriesId);

        series.Should().NotBeNull();
        series!.Title.Should().Be("Updated Series");
        series.AlternativeTitles.Should().BeEquivalentTo(["Alt 1", "Alt 2"]);
        series.Authors.Should().BeEquivalentTo(["Auth 1"]);
        series.StartTime.Should().BeWithin(TimeSpan.FromSeconds(100));
        series.Synopsis.Should().Be("Updated Series Description");
        series.Thumbnail.Should().Be("Updated Series Thumbnail");
        series.Type.Should().Be(SeriesType.Comic);
        series.Status.Should().Be(SeriesStatus.Ongoing);
        series.ContentRating.Should().Be(ContentRating.Mature);
        series.VolumeCount.Should().Be(0);
        series.Genres.Should().HaveCount(5);
        series.LastModifiedBy.Should().Be(userId);
        series.LastModified.Should().BeWithin(TimeSpan.FromMilliseconds(100));
    }

    [Theory]
    [MemberData(nameof(UpdateSeriesShouldRequireValidAttributesTestCases))]
    public async Task UpdateSeriesShouldRequireValidAttributes(
        string title,
        string synopsis,
        string thumbnail,
        SeriesStatus status, int[] genres,
        ContentQuestionnaire[] categoryRatings)
    {
        // Arrange
        await RunAsDefaultUserAsync();

        int seriesId = await SendAsync(new CreateSeriesCommand(
            "Test Series",
            "Test Series Description",
            "Test Series Thumbnail",
            SeriesType.Comic,
            [1, 2, 3],
            [
                new ContentQuestionnaire(ContentCategory.Violent, 1),
                new ContentQuestionnaire(ContentCategory.Nudity, 2),
                new ContentQuestionnaire(ContentCategory.Profanity, 1),
                new ContentQuestionnaire(ContentCategory.Alcohol, 2),
                new ContentQuestionnaire(ContentCategory.Sensitive, 2),
                new ContentQuestionnaire(ContentCategory.Sexual, 2)
            ],
            [],
            [],
            null
        ));

        // AAA
        await FluentActions.Invoking(async () => await SendAsync(new UpdateSeriesCommand(
                seriesId,
                title,
                synopsis,
                thumbnail,
                status,
                genres,
                categoryRatings,
                [],
                [],
                null)))
            .Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task UpdateSeriesShouldRequireExistingSeries()
    {
        // Arrange
        await RunAsDefaultUserAsync();

        UpdateSeriesCommand command = new(
            1,
            "Test Series",
            "Test Series Description",
            "Test Series Thumbnail",
            SeriesStatus.Completed,
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
            null
        );

        // Act & Assert
        await FluentActions.Invoking(async () => await SendAsync(command))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateSeriesShouldRequireValidUser()
    {
        // Arrange
        await RunAsDefaultUserAsync();

        int seriesId = await SendAsync(new CreateSeriesCommand(
            "Test Series",
            "Test Series Description",
            "Test Series Thumbnail",
            SeriesType.Comic,
            [1, 2, 3],
            [
                new ContentQuestionnaire(ContentCategory.Violent, 1),
                new ContentQuestionnaire(ContentCategory.Nudity, 2),
                new ContentQuestionnaire(ContentCategory.Profanity, 1),
                new ContentQuestionnaire(ContentCategory.Alcohol, 2),
                new ContentQuestionnaire(ContentCategory.Sensitive, 2),
                new ContentQuestionnaire(ContentCategory.Sexual, 2)
            ],
            [],
            [],
            null
        ));

        UpdateSeriesCommand? updateCommand = new(
            seriesId,
            "Updated Series",
            "Updated Series Description",
            "Updated Series Thumbnail",
            SeriesStatus.Ongoing,
            [1, 2, 3, 4],
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
            null
        );

        await RunAsUserAsync("AnotherGuy", "Itsapassword1!", []);

        // Act & Assert
        await FluentActions.Invoking(async () => await SendAsync(updateCommand))
            .Should().ThrowAsync<ForbiddenAccessException>();
    }
}
