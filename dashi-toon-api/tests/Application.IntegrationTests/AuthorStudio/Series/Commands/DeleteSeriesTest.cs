using DashiToon.Api.Application.AuthorStudio.Series.Commands.CreateSeries;
using DashiToon.Api.Application.AuthorStudio.Series.Commands.DeleteSeries;
using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Domain.Enums;

namespace Application.IntegrationTests.AuthorStudio.Series.Commands;

using static Testing;

[Collection("Serialize")]
public class DeleteSeriesTest : BaseIntegrationTest
{
    public DeleteSeriesTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task DeleteSeriesShouldRemoveFromDatabase()
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
                new ContentQuestionnaire(ContentCategory.Alcohol, 3),
                new ContentQuestionnaire(ContentCategory.Sensitive, 1),
                new ContentQuestionnaire(ContentCategory.Sexual, 2)
            ],
            [],
            [],
            null
        ));

        DeleteSeriesCommand deleteSeriesCommand = new(seriesId);
        // Act
        await SendAsync(deleteSeriesCommand);

        // Assert
        DashiToon.Api.Domain.Entities.Series? series = await FindAsync<DashiToon.Api.Domain.Entities.Series>(seriesId);

        series.Should().BeNull();
    }

    [Fact]
    public async Task DeleteSeriesShouldRequireExistingSeries()
    {
        await RunAsDefaultUserAsync();

        DeleteSeriesCommand deleteSeriesCommand = new(1);

        await FluentActions.Invoking(async () => await SendAsync(deleteSeriesCommand)).Should()
            .ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DeleteSeriesShouldRequireValidUser()
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
                new ContentQuestionnaire(ContentCategory.Alcohol, 3),
                new ContentQuestionnaire(ContentCategory.Sensitive, 1),
                new ContentQuestionnaire(ContentCategory.Sexual, 2)
            ],
            [],
            [],
            null
        ));

        await RunAsUserAsync("AnotherGuy", "Itsapassword1!", []);

        DeleteSeriesCommand? deleteSeriesCommand = new(seriesId);

        // Act & Assert
        await FluentActions.Invoking(async () => await SendAsync(deleteSeriesCommand)).Should()
            .ThrowAsync<ForbiddenAccessException>();
    }
}
