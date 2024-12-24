using DashiToon.Api.Application.AuthorStudio.Series.Commands.CreateSeries;
using DashiToon.Api.Application.AuthorStudio.Volumes.Commands.CreateVolume;
using DashiToon.Api.Application.AuthorStudio.Volumes.Commands.UpdateVolume;
using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Enums;

namespace Application.IntegrationTests.AuthorStudio.Volumes.Commands;

using static Testing;

[Collection("Serialize")]
public class UpdateVolumeTest : BaseIntegrationTest
{
    public UpdateVolumeTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task UpdateVolumesShouldUpdateSuccessfully()
    {
        // Arrange
        string userId = await RunAsDefaultUserAsync();

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

        int volumeId = await SendAsync(createVolumeCommand);

        UpdateVolumeCommand updateVolumeCommand = new(seriesId, volumeId, "Bruh", "Lmao");

        // Act
        await SendAsync(updateVolumeCommand);

        // Assert
        IApplicationDbContext context = GetContext();

        Volume? volume = context.Volumes.FirstOrDefault(x => x.Id == volumeId);

        volume.Should().NotBeNull();
        volume!.Name.Should().Be("Bruh");
        volume.Introduction.Should().Be("Lmao");
    }

    [Fact]
    public async Task UpdateVolumesShouldRequireValidSeries()
    {
        // Arrange
        await RunAsDefaultUserAsync();

        UpdateVolumeCommand updateVolumeCommand = new(1, 1, "TestName", "TestIntroduction");

        // Act
        await FluentActions.Invoking(async () => await SendAsync(updateVolumeCommand)).Should()
            .ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateVolumesShouldRequireValidVolume()
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

        UpdateVolumeCommand updateVolumeCommand = new(seriesId, 1, "TestName", "TestIntroduction");

        // Act
        await FluentActions.Invoking(async () => await SendAsync(updateVolumeCommand)).Should()
            .ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateVolumeShouldRequireValidUser()
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

        int volumeId = await SendAsync(createVolumeCommand);

        await RunAsUserAsync("AnotherGuy", "Itsapassword1!", []);

        UpdateVolumeCommand updateVolumeCommand = new(seriesId, volumeId, "TestName", "TestIntroduction");

        // Act & Assert
        await FluentActions.Invoking(async () => await SendAsync(updateVolumeCommand)).Should()
            .ThrowAsync<ForbiddenAccessException>();
    }
}
