using DashiToon.Api.Application.Administrator.Missions.Commands.CreateMission;
using DashiToon.Api.Application.Administrator.Missions.Commands.UpdateMissionStatus;
using DashiToon.Api.Application.Administrator.Missions.Models;

namespace Application.IntegrationTests.Administrator.Missions.Commands;

using static Testing;

[Collection("Serialize")]
public class UpdateMissionStatusTest : BaseIntegrationTest
{
    public UpdateMissionStatusTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task UpdateMissionStatusShouldChangeMissionStatus()
    {
        // Arrange
        await RunAsAdministratorAsync();

        int readCount = 2;
        int reward = 20;

        MissionVm? mission = await SendAsync(new CreateMissionCommand(readCount, reward));

        // Act
        MissionVm? updatedMission = await SendAsync(new UpdateMissionStatusCommand(mission.Id, false));

        // Assert
        updatedMission.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateMissionStatusShouldRequireExistingMission()
    {
        // Arrange
        await RunAsAdministratorAsync();

        // Act & Assert
        await FluentActions.Invoking(() => SendAsync(new UpdateMissionStatusCommand(Guid.NewGuid(), false)))
            .Should().ThrowAsync<NotFoundException>();
    }
}
