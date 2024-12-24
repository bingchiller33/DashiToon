using DashiToon.Api.Application.Administrator.Missions.Commands.CreateMission;
using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.CreateComicChapter;
using DashiToon.Api.Application.AuthorStudio.Chapters.Commands.PublishChapter;
using DashiToon.Api.Application.AuthorStudio.Series.Commands.CreateSeries;
using DashiToon.Api.Application.AuthorStudio.Series.Commands.UpdateSeries;
using DashiToon.Api.Application.AuthorStudio.Volumes.Commands.CreateVolume;
using DashiToon.Api.Application.ReadContent.Queries.GetComicChapter;
using DashiToon.Api.Application.Users.Commands.FollowSeries;
using DashiToon.Api.Application.Users.Queries.GetMissions;
using DashiToon.Api.Domain.Enums;

namespace Application.IntegrationTests.Users.Queries;

using static Testing;

[Collection("Serialize")]
public class GetMissionTest : BaseIntegrationTest
{
    public GetMissionTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task GetMissionsShouldReturnActiveMissions()
    {
        // Arrange
        await RunAsAdministratorAsync();

        DashiToon.Api.Application.Administrator.Missions.Models.MissionVm? mission1 =
            await SendAsync(new CreateMissionCommand(3, 50));
        DashiToon.Api.Application.Administrator.Missions.Models.MissionVm? mission2 =
            await SendAsync(new CreateMissionCommand(5, 50));
        DashiToon.Api.Application.Administrator.Missions.Models.MissionVm? mission3 =
            await SendAsync(new CreateMissionCommand(10, 50, false));

        await RunAsDefaultUserAsync();

        List<MissionVm>? expected = new()
        {
            new MissionVm(mission1.Id, 50, 3, false, false), new MissionVm(mission2.Id, 50, 5, false, false)
        };

        // Act
        List<MissionVm>? missions = await SendAsync(new GetMissionsQuery());

        // Assert
        missions.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GetMissionsShouldShowCompletionStatusForCurrentUser()
    {
        // Arrange
        await RunAsAdministratorAsync();

        DashiToon.Api.Application.Administrator.Missions.Models.MissionVm? mission1 =
            await SendAsync(new CreateMissionCommand(1, 50));
        DashiToon.Api.Application.Administrator.Missions.Models.MissionVm? mission2 =
            await SendAsync(new CreateMissionCommand(2, 50));

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
                new ContentQuestionnaire(ContentCategory.Nudity, 2),
                new ContentQuestionnaire(ContentCategory.Profanity, 1),
                new ContentQuestionnaire(ContentCategory.Alcohol, 3),
                new ContentQuestionnaire(ContentCategory.Sensitive, 1),
                new ContentQuestionnaire(ContentCategory.Sexual, 2)
            ],
            [],
            [],
            null));

        int volumeId = await SendAsync(new CreateVolumeCommand(seriesId, "Vol1", string.Empty));

        int chapterId = await SendAsync(new CreateComicChapterCommand(
            seriesId,
            volumeId,
            "TestTitle1",
            "TestThumbnail1",
            ["image1.png", "image2.png"],
            "TestNote1"
        ));

        await SendAsync(new PublishChapterCommand(seriesId, volumeId, chapterId));

        await SendAsync(new FollowSeriesCommand(seriesId));

        await SendAsync(new GetComicChapterQuery(seriesId, volumeId, chapterId));

        List<MissionVm>? expected = new()
        {
            new MissionVm(mission1.Id, 50, 1, false, true), new MissionVm(mission2.Id, 50, 2, false, false)
        };
        // Act
        List<MissionVm>? missions = await SendAsync(new GetMissionsQuery());

        // Assert
        missions.Should().BeEquivalentTo(expected);
    }
}
