using DashiToon.Api.Application.AuthorStudio.Series.Commands.CreateSeries;
using DashiToon.Api.Application.AuthorStudio.Volumes.Commands.CreateVolume;
using DashiToon.Api.Application.AuthorStudio.Volumes.Commands.DeleteVolume;
using DashiToon.Api.Application.AuthorStudio.Volumes.Commands.UpdateVolume;
using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Enums;

namespace Application.IntegrationTests.AuthorStudio.Volumes.Commands;

using static Testing;

[Collection("Serialize")]
public class DeleteVolumeTest : BaseIntegrationTest
{
    public DeleteVolumeTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task DeleteVolumeShouldRemoveFromDatabase()
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

        DeleteVolumeCommand deleteVolumeCommand = new(seriesId, volumeId);

        // Act
        await SendAsync(deleteVolumeCommand);

        // Assert
        IApplicationDbContext context = GetContext();

        Volume? volume = context.Volumes.FirstOrDefault(x => x.Id == volumeId);
        DashiToon.Api.Domain.Entities.Series? series1 = context.Series.FirstOrDefault(x => x.Id == seriesId);

        volume.Should().BeNull();
        series1!.VolumeCount.Should().Be(0);
    }

    [Fact]
    public async Task DeleteVolumeShouldRequireValidSeries()
    {
        // Arrange
        await RunAsDefaultUserAsync();

        DeleteVolumeCommand deleteVolumeCommand = new(1, 1);

        // Act
        await FluentActions.Invoking(async () => await SendAsync(deleteVolumeCommand)).Should()
            .ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DeleteVolumeShouldRequireValidVolume()
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

        DeleteVolumeCommand deleteVolumeCommand = new(seriesId, 1);

        // Act
        await FluentActions.Invoking(async () => await SendAsync(deleteVolumeCommand)).Should()
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

        DeleteVolumeCommand deleteVolumeCommand = new(seriesId, volumeId);

        // Act & Assert
        await FluentActions.Invoking(async () => await SendAsync(deleteVolumeCommand)).Should()
            .ThrowAsync<ForbiddenAccessException>();
    }
}
