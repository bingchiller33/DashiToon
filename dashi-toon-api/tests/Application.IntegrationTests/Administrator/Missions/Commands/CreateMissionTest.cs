using DashiToon.Api.Application.Administrator.Missions.Commands.CreateMission;
using DashiToon.Api.Application.Administrator.Missions.Models;

namespace Application.IntegrationTests.Administrator.Missions.Commands;

using static Testing;

[Collection("Serialize")]
public class CreateMissionTest : BaseIntegrationTest
{
    public CreateMissionTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task CreateMissionShouldSucceed()
    {
        // Arrange
        await RunAsAdministratorAsync();

        int readCount = 2;
        int reward = 20;

        // Act
        MissionVm? mission = await SendAsync(new CreateMissionCommand(readCount, reward));

        // Assert
        mission.Should().NotBeNull();
        mission.ReadCount.Should().Be(readCount);
        mission.Reward.Should().Be(reward);
        mission.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CreateMissionShouldNotAllowedDuplicateReadCountMission()
    {
        // Arrange
        await RunAsAdministratorAsync();

        int readCount = 2;
        int reward = 20;

        await SendAsync(new CreateMissionCommand(readCount, reward));

        // Act & Assert
        await FluentActions.Invoking(() => SendAsync(new CreateMissionCommand(readCount, reward)))
            .Should().ThrowAsync<ArgumentException>();
    }
}
