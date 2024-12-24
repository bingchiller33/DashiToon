using DashiToon.Api.Application.AuthorStudio.Series.Commands.CreateSeries;
using DashiToon.Api.Application.AuthorStudio.Volumes.Commands.CreateVolume;
using DashiToon.Api.Application.AuthorStudio.Volumes.Queries.GetVolume;
using DashiToon.Api.Application.AuthorStudio.Volumes.Queries.GetVolumes;
using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Domain.Enums;

namespace Application.IntegrationTests.AuthorStudio.Volumes.Queries;

using static Testing;

[Collection("Serialize")]
public class GetVolumeTest : BaseIntegrationTest
{
    public GetVolumeTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task GetVolumeShouldReturnVolumeDetail()
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
            ], [],
            [],
            null));

        CreateVolumeCommand createVolume1Command = new(seriesId, "TestName1", "TestIntroduction1");
        CreateVolumeCommand createVolume2Command = new(seriesId, "TestName2", "TestIntroduction2");
        int volume1Id = await SendAsync(createVolume1Command);
        int volume2Id = await SendAsync(createVolume2Command);

        // Act
        VolumeDetailVm volumeDetail = await SendAsync(new GetVolumeQuery(seriesId, volume1Id));

        // Assert

        volumeDetail.Should().NotBeNull();
        volumeDetail.VolumeId.Should().Be(volume1Id);
        volumeDetail.VolumeNumber.Should().Be(1);
        volumeDetail.Name.Should().Be("TestName1");
        volumeDetail.Introduction.Should().Be("TestIntroduction1");
        volumeDetail.ChapterCount.Should().Be(0);
    }

    [Fact]
    public async Task GetVolumeShouldRequireExistingVolume()
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
            ], [],
            [],
            null));

        CreateVolumeCommand createVolume1Command = new(seriesId, "TestName1", "TestIntroduction1");
        int volume1Id = await SendAsync(createVolume1Command);

        // Act & Assert
        await FluentActions.Invoking(() => SendAsync(new GetVolumeQuery(seriesId, 2)))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetVolumeShouldRequireValidSeries()
    {
        // Arrange
        string userId = await RunAsDefaultUserAsync();

        // Act
        await FluentActions.Invoking(() => SendAsync(new GetVolumeQuery(1, 1)))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetVolumeShouldRequireValidUser()
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
            ], [],
            [],
            null));

        await RunAsUserAsync("AnotherGuy", "Itsapassword1!", []);

        // Act
        await FluentActions.Invoking(() => SendAsync(new GetVolumeQuery(seriesId, 1)))
            .Should().ThrowAsync<ForbiddenAccessException>();
    }
}
