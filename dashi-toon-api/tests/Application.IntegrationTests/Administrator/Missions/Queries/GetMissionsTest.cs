using DashiToon.Api.Application.Administrator.Missions.Commands.CreateMission;
using DashiToon.Api.Application.Administrator.Missions.Models;
using GetMissionsQuery = DashiToon.Api.Application.Administrator.Missions.Queries.GetMissions.GetMissionsQuery;

namespace Application.IntegrationTests.Administrator.Missions.Queries;

using static Testing;

[Collection("Serialize")]
public class GetMissionsTest : BaseIntegrationTest
{
    public GetMissionsTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task GetMissionsShouldReturnMissions()
    {
        // Arrange
        await RunAsAdministratorAsync();

        MissionVm? mission1 = await SendAsync(new CreateMissionCommand(3, 50));
        MissionVm? mission2 = await SendAsync(new CreateMissionCommand(5, 50));

        List<MissionVm>? expected = new() { mission1, mission2 };
        // Act
        List<MissionVm>? missions = await SendAsync(new GetMissionsQuery());

        // Assert
        missions.Should().BeEquivalentTo(expected);
    }
}
