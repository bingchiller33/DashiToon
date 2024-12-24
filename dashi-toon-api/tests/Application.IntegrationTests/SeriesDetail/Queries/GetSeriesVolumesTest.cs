using DashiToon.Api.Application.AuthorStudio.Series.Commands.CreateSeries;
using DashiToon.Api.Application.AuthorStudio.Series.Commands.UpdateSeries;
using DashiToon.Api.Application.AuthorStudio.Volumes.Commands.CreateVolume;
using DashiToon.Api.Application.SeriesDetail.Queries.GetSeriesVolumes;
using DashiToon.Api.Domain.Enums;

namespace Application.IntegrationTests.SeriesDetail.Queries;

using static Testing;

[Collection("Serialize")]
public class GetSeriesVolumesTest : BaseIntegrationTest
{
    public GetSeriesVolumesTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task GetSeriesVolumeShouldReturnCorrectVolumes()
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
            [],
            [],
            null));

        int volume1Id = await SendAsync(new CreateVolumeCommand(seriesId, "TestVolume1", "TestIntroduction1"));
        int volume2Id = await SendAsync(new CreateVolumeCommand(seriesId, "TestVolume2", "TestIntroduction2"));
        int volume3Id = await SendAsync(new CreateVolumeCommand(seriesId, "TestVolume3", "TestIntroduction3"));

        List<SeriesVolumeVm> expected = new()
        {
            new SeriesVolumeVm(volume3Id, 3, "TestVolume3", "TestIntroduction3"),
            new SeriesVolumeVm(volume2Id, 2, "TestVolume2", "TestIntroduction2"),
            new SeriesVolumeVm(volume1Id, 1, "TestVolume1", "TestIntroduction1")
        };

        // Act
        List<SeriesVolumeVm> volumes = await SendAsync(new GetSeriesVolumesQuery(seriesId));

        // Assert
        volumes.Should().HaveCount(3);
        volumes.Should().ContainInConsecutiveOrder(expected);
    }

    [Fact]
    public async Task GetSeriesVolumeShouldRequireExistingSeries()
    {
        // Act
        List<SeriesVolumeVm> volumes = await SendAsync(new GetSeriesVolumesQuery(1));

        // Assert
        volumes.Should().BeEmpty();
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

        await SendAsync(new CreateVolumeCommand(seriesId, "TestVolume1", "TestIntroduction1"));
        await SendAsync(new CreateVolumeCommand(seriesId, "TestVolume2", "TestIntroduction2"));
        await SendAsync(new CreateVolumeCommand(seriesId, "TestVolume3", "TestIntroduction3"));

        // Act
        List<SeriesVolumeVm> volumes = await SendAsync(new GetSeriesVolumesQuery(seriesId));

        // Assert
        volumes.Should().BeEmpty();
    }
}
