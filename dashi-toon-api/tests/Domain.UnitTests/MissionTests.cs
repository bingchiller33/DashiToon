using DashiToon.Api.Domain.Entities;
using FluentAssertions;

namespace Domain.UnitTests;

public class MissionTests
{
    [Fact]
    public void CreateMissionShouldCreateSuccessfully()
    {
        // Arrange
        int readCount = 1;
        int reward = 30;
        bool isActive = true;

        // Act
        Mission mission = Mission.CreateNew(readCount, reward, isActive);

        // Assert
        mission.Should().NotBeNull();
        mission.ReadCount.Should().Be(readCount);
        mission.Reward.Should().Be(reward);
        mission.IsActive.Should().Be(isActive);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CreateMissionShouldRequireValidReadCount(int readCount)
    {
        // Arrange
        FluentActions.Invoking(() => Mission.CreateNew(readCount, 10, true))
            .Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CreateMissionShouldRequireValidReadCountAndReward(int reward)
    {
        // Arrange
        FluentActions.Invoking(() => Mission.CreateNew(3, reward, true))
            .Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateMissionShouldUpdateSuccessfully()
    {
        // Arrange
        Mission? mission = Mission.CreateNew(1, 30, false);

        int readCount = 10;
        int reward = 100;
        bool isActive = true;

        // Act
        mission.Update(readCount, reward, isActive);

        // Assert
        mission.Should().NotBeNull();
        mission.ReadCount.Should().Be(readCount);
        mission.Reward.Should().Be(reward);
        mission.IsActive.Should().Be(isActive);
    }

    [Fact]
    public void UpdateMissionStatusShouldUpdateStatus()
    {
        // Arrange
        Mission? mission = Mission.CreateNew(1, 30, false);

        // Act
        mission.UpdateStatus(true);

        // Assert
        mission.IsActive.Should().BeTrue();
    }
}
