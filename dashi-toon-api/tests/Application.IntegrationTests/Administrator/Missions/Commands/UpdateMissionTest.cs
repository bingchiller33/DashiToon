using DashiToon.Api.Application.Administrator.Missions.Commands.CreateMission;
using DashiToon.Api.Application.Administrator.Missions.Commands.UpdateMission;
using DashiToon.Api.Application.Administrator.Missions.Models;

namespace Application.IntegrationTests.Administrator.Missions.Commands;

using static Testing;

[Collection("Serialize")]
public class UpdateMissionTest : BaseIntegrationTest
{
    public UpdateMissionTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task UpdateMissionShouldSucceed()
    {
        // Arrange
        await RunAsAdministratorAsync();

        int readCount = 2;
        int reward = 20;

        MissionVm? mission = await SendAsync(new CreateMissionCommand(readCount, reward));

        // Act
        MissionVm? updatedMission = await SendAsync(new UpdateMissionCommand(mission.Id, 3, 50, false));

        // Assert
        updatedMission.Should().NotBeNull();
        updatedMission.ReadCount.Should().Be(3);
        updatedMission.Reward.Should().Be(50);
        updatedMission.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateMissionShouldNotAllowedDuplicateReadCountMission()
    {
        // Arrange
        await RunAsAdministratorAsync();

        MissionVm? mission1 = await SendAsync(new CreateMissionCommand(2, 20));

        MissionVm? mission2 = await SendAsync(new CreateMissionCommand(3, 50));

        // Act & Assert
        await FluentActions.Invoking(() => SendAsync(new UpdateMissionCommand(mission2.Id, 2, 30, true)))
            .Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task UpdateMissionShouldRequireExistingMission()
    {
        // Arrange
        await RunAsAdministratorAsync();

        // Act & Assert
        await FluentActions.Invoking(() => SendAsync(new UpdateMissionCommand(Guid.NewGuid(), 2, 30, true)))
            .Should().ThrowAsync<NotFoundException>();
    }
}
