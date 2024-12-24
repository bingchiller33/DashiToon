using DashiToon.Api.Application.AuthorStudio.Series.Commands.CreateSeries;
using DashiToon.Api.Domain.Enums;

namespace Application.IntegrationTests.AuthorStudio.Series.Commands;

using static Testing;

[Collection("Serialize")]
public class CreateSeriesTest : BaseIntegrationTest
{
    public CreateSeriesTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    public static IEnumerable<object[]> CreateSeriesShouldRequireValidAttributesTestCases =>
    [
        [
            new CreateSeriesCommand( // Invalid rating option
                "TestTitle",
                "TestSynopsis",
                "TestThumbnail",
                SeriesType.Comic,
                [1, 2, 3],
                [
                    new ContentQuestionnaire(ContentCategory.Violent, -1),
                    new ContentQuestionnaire(ContentCategory.Nudity, 3),
                    new ContentQuestionnaire(ContentCategory.Profanity, 1),
                    new ContentQuestionnaire(ContentCategory.Alcohol, -3),
                    new ContentQuestionnaire(ContentCategory.Sensitive, 1),
                    new ContentQuestionnaire(ContentCategory.Sexual, -2)
                ],
                [],
                [],
                null)
        ],
        [
            new CreateSeriesCommand( // Invalid rating option
                "TestTitle",
                "TestSynopsis",
                "TestThumbnail",
                SeriesType.Comic,
                [1, 2, 3],
                [
                    new ContentQuestionnaire(ContentCategory.Violent, 0),
                    new ContentQuestionnaire(ContentCategory.Nudity, 2),
                    new ContentQuestionnaire(ContentCategory.Profanity, 1),
                    new ContentQuestionnaire(ContentCategory.Alcohol, 2),
                    new ContentQuestionnaire(ContentCategory.Sensitive, 3),
                    new ContentQuestionnaire(ContentCategory.Sexual, 1),
                    new ContentQuestionnaire(ContentCategory.Sexual, 2)
                ],
                [],
                [],
                null)
        ],
        [
            new CreateSeriesCommand( // Invalid rating option
                "TestTitle",
                "TestSynopsis",
                "TestThumbnail",
                SeriesType.Comic,
                [1, 2, 3],
                [
                    new ContentQuestionnaire(ContentCategory.Violent, 0),
                    new ContentQuestionnaire(ContentCategory.Nudity, 2),
                    new ContentQuestionnaire(ContentCategory.Profanity, 1),
                    new ContentQuestionnaire(ContentCategory.Alcohol, 2)
                ],
                [],
                [],
                null)
        ],
        [
            new CreateSeriesCommand( // Invalid Title
                string.Empty,
                "TestSynopsis",
                "TestThumbnail",
                SeriesType.Comic,
                [1, 2, 3],
                [
                    new ContentQuestionnaire(ContentCategory.Violent, 0),
                    new ContentQuestionnaire(ContentCategory.Nudity, 0),
                    new ContentQuestionnaire(ContentCategory.Profanity, 0),
                    new ContentQuestionnaire(ContentCategory.Alcohol, 0),
                    new ContentQuestionnaire(ContentCategory.Sensitive, 0),
                    new ContentQuestionnaire(ContentCategory.Sexual, 0)
                ],
                [],
                [],
                null)
        ],
        [
            new CreateSeriesCommand( // Invalid Title
                new string('a', 256),
                "TestSynopsis",
                "TestThumbnail",
                SeriesType.Comic,
                [1, 2, 3],
                [
                    new ContentQuestionnaire(ContentCategory.Violent, 0),
                    new ContentQuestionnaire(ContentCategory.Nudity, 0),
                    new ContentQuestionnaire(ContentCategory.Profanity, 0),
                    new ContentQuestionnaire(ContentCategory.Alcohol, 0),
                    new ContentQuestionnaire(ContentCategory.Sensitive, 0),
                    new ContentQuestionnaire(ContentCategory.Sexual, 0)
                ],
                [],
                [],
                null)
        ],
        [
            new CreateSeriesCommand( // Invalid Synopsis
                "TestTitle",
                string.Empty,
                "TestThumbnail",
                SeriesType.Comic,
                [1, 2, 3],
                [
                    new ContentQuestionnaire(ContentCategory.Violent, 0),
                    new ContentQuestionnaire(ContentCategory.Nudity, 0),
                    new ContentQuestionnaire(ContentCategory.Profanity, 0),
                    new ContentQuestionnaire(ContentCategory.Alcohol, 0),
                    new ContentQuestionnaire(ContentCategory.Sensitive, 0),
                    new ContentQuestionnaire(ContentCategory.Sexual, 0)
                ],
                [],
                [],
                null)
        ],
        [
            new CreateSeriesCommand( // Invalid Synopsis
                "TestTitle",
                new string('a', 5001),
                "TestThumbnail",
                SeriesType.Comic,
                [1, 2, 3],
                [
                    new ContentQuestionnaire(ContentCategory.Violent, 0),
                    new ContentQuestionnaire(ContentCategory.Nudity, 0),
                    new ContentQuestionnaire(ContentCategory.Profanity, 0),
                    new ContentQuestionnaire(ContentCategory.Alcohol, 0),
                    new ContentQuestionnaire(ContentCategory.Sensitive, 0),
                    new ContentQuestionnaire(ContentCategory.Sexual, 0)
                ],
                [],
                [],
                null)
        ],
        [
            new CreateSeriesCommand( // Invalid Genres
                "TestTitle",
                "TestSynopsis",
                "TestThumbnail",
                SeriesType.Comic,
                [],
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
                null)
        ]
    ];

    [Fact]
    public async Task CreateSeriesShouldPersistToDatabase()
    {
        // Arrange
        string userId = await RunAsDefaultUserAsync();

        CreateSeriesCommand createSeriesCommand = new(
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
            null);

        // Act
        int seriesId = await SendAsync(createSeriesCommand);

        // Assert
        DashiToon.Api.Domain.Entities.Series? series = await FindAsync<DashiToon.Api.Domain.Entities.Series>(seriesId);

        series.Should().NotBeNull();
        series!.Title.Should().Be("TestTitle");
        series.Synopsis.Should().Be("TestSynopsis");
        series.Thumbnail.Should().Be("TestThumbnail");
        series.Type.Should().Be(SeriesType.Comic);
        series.Status.Should().Be(SeriesStatus.Draft);
        series.ContentRating.Should().Be(ContentRating.Mature);
        series.VolumeCount.Should().Be(0);
        series.Created.Should().BeWithin(TimeSpan.FromMilliseconds(100));
        series.CreatedBy.Should().Be(userId);
    }

    [Theory]
    [MemberData(nameof(CreateSeriesShouldRequireValidAttributesTestCases))]
    public async Task CreateSeriesShouldRequireValidAttributes(CreateSeriesCommand command)
    {
        // AAA
        await RunAsDefaultUserAsync();

        await FluentActions.Invoking(async () => await SendAsync(command)).Should().ThrowAsync<ArgumentException>();
    }
}
