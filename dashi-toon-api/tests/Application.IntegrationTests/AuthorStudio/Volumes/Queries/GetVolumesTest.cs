using DashiToon.Api.Application.AuthorStudio.Series.Commands.CreateSeries;
using DashiToon.Api.Application.AuthorStudio.Volumes.Commands.CreateVolume;
using DashiToon.Api.Application.AuthorStudio.Volumes.Queries.GetVolumes;
using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Domain.Enums;

namespace Application.IntegrationTests.AuthorStudio.Volumes.Queries;

using static Testing;

[Collection("Serialize")]
public class GetVolumesTest : BaseIntegrationTest
{
    public GetVolumesTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task GetVolumesShouldReturnAllVolumes()
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
            ],
            [],
            [],
            null));

        CreateVolumeCommand createVolume1Command = new(seriesId, "TestName1", "TestIntroduction1");
        CreateVolumeCommand createVolume2Command = new(seriesId, "TestName2", "TestIntroduction2");
        int volume1Id = await SendAsync(createVolume1Command);
        int volume2Id = await SendAsync(createVolume2Command);

        // Act
        List<VolumeVm> result = await SendAsync(new GetVolumesQuery(seriesId));

        // Assert
        VolumeVm[] expected = new[]
        {
            new VolumeVm(volume1Id, 1, "TestName1", "TestIntroduction1", 0),
            new VolumeVm(volume2Id, 2, "TestName2", "TestIntroduction2", 0)
        };

        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GetAllVolumesShouldRequireValidSeries()
    {
        // Arrange
        string userId = await RunAsDefaultUserAsync();

        // Act
        await FluentActions.Invoking(() => SendAsync(new GetVolumesQuery(1)))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetAllVolumesShouldRequireValidUser()
    {
        // Arrange
        await RunAsDefaultUserAsync();

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
            ],
            [],
            [],
            null));

        await RunAsUserAsync("AnotherGuy", "Itsapassword1!", []);

        // Act
        await FluentActions.Invoking(() => SendAsync(new GetVolumesQuery(seriesId)))
            .Should().ThrowAsync<ForbiddenAccessException>();
    }
}
