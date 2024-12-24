using DashiToon.Api.Application.AuthorStudio.Series.Commands.CreateSeries;
using DashiToon.Api.Application.AuthorStudio.Volumes.Commands.CreateVolume;
using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.IntegrationTests.AuthorStudio.Volumes.Commands;

using static Testing;

[Collection("Serialize")]
public class CreateVolumesTest : BaseIntegrationTest
{
    public CreateVolumesTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task CreateVolumesShouldPersistToDatabase()
    {
        // Arrange
        await RunAsDefaultUserAsync();

        CreateSeriesCommand createSeriesCommand = new(
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
            null);

        int seriesId = await SendAsync(createSeriesCommand);

        CreateVolumeCommand createVolumeCommand = new(seriesId, "TestName", "TestIntroduction");

        // Act
        await SendAsync(createVolumeCommand);

        // Assert
        IApplicationDbContext context = GetContext();

        DashiToon.Api.Domain.Entities.Series? series = context.Series
            .Include(x => x.Volumes)
            .FirstOrDefault(x => x.Id == seriesId);

        series.Should().NotBeNull();
        series!.Volumes.Should().HaveCount(1);
        series.Volumes[0].Name.Should().Be("TestName");
        series.Volumes[0].Introduction.Should().Be("TestIntroduction");
        series.Volumes[0].SeriesId.Should().Be(seriesId);
        series.Volumes[0].VolumeNumber.Should().Be(1);
    }

    [Fact]
    public async Task CreateVolumesShouldRequireValidSeries()
    {
        // Arrange
        await RunAsDefaultUserAsync();

        CreateVolumeCommand createVolumeCommand = new(1, "TestName", "TestIntroduction");

        // Act
        await FluentActions.Invoking(async () => await SendAsync(createVolumeCommand)).Should()
            .ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateVolumesShouldRequireValidUser()
    {
        // Arrange
        await RunAsDefaultUserAsync();

        CreateSeriesCommand createSeriesCommand = new(
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
            null);

        int seriesId = await SendAsync(createSeriesCommand);

        CreateVolumeCommand createVolumeCommand = new(seriesId, "TestName", "TestIntroduction");

        await RunAsUserAsync("AnotherGuy", "Itsapassword1!", []);

        // Act & Assert
        await FluentActions.Invoking(async () => await SendAsync(createVolumeCommand)).Should()
            .ThrowAsync<ForbiddenAccessException>();
    }
}
