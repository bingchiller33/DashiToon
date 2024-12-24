using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Exceptions;
using DashiToon.Api.Domain.Services;
using DashiToon.Api.Infrastructure.Identity;
using FluentAssertions;

namespace Domain.UnitTests;

public class MissionServiceTests
{
    [Fact]
    public void DailyCheckinShouldTopUpKanaCoinForUser()
    {
        // Arrange
        ApplicationUser user = new();

        // Act
        MissionService.DailyCheckin(user);

        // Assert
        user.Ledgers.Should().HaveCount(1);
        user.KanaCoin.Should().Be(100);
    }

    [Fact]
    public void DailyCheckinInShouldNotAllowUserToCheckinTwiceInADay()
    {
        // Arrange
        ApplicationUser user = new();

        MissionService.DailyCheckin(user);

        // Act & Assert
        FluentActions.Invoking(() => MissionService.DailyCheckin(user))
            .Should().Throw<AlreadyCheckinException>();
    }

    [Fact]
    public void CompleteMissionShouldTopUpKanaCoinForUser()
    {
        // Arrange
        ApplicationUser? user = new();

        Mission? mission = Mission.CreateNew(4, 69, true);

        user.ReadChapter(1);
        user.ReadChapter(2);
        user.ReadChapter(3);
        user.ReadChapter(4);

        // Act
        MissionService.CompleteMission(user, mission);

        // Assert
        user.Ledgers.Should().HaveCount(1);
        user.KanaCoin.Should().Be(69);
    }

    [Fact]
    public void CompletedMissionShouldNotTopUpKanaCoinForUser()
    {
        // Arrange
        ApplicationUser? user = new();

        Mission? mission = Mission.CreateNew(4, 69, true);

        user.ReadChapter(1);
        user.ReadChapter(2);
        user.ReadChapter(3);
        user.ReadChapter(4);

        MissionService.CompleteMission(user, mission);

        // Act
        MissionService.CompleteMission(user, mission);

        // Assert
        user.Ledgers.Should().HaveCount(1);
        user.KanaCoin.Should().Be(69);
    }

    [Fact]
    public void UnfinishedMissionShouldNotTopUpKanaCoinForUser()
    {
        // Arrange
        ApplicationUser? user = new();

        Mission? mission = Mission.CreateNew(4, 69, true);

        user.ReadChapter(1);

        // Act
        MissionService.CompleteMission(user, mission);

        // Assert
        user.Ledgers.Should().BeEmpty();
        user.KanaCoin.Should().Be(0);
    }

    [Fact]
    public void InactiveMissionShouldNotTopUpKanaCoinForUser()
    {
        // Arrange
        ApplicationUser? user = new();

        Mission? mission = Mission.CreateNew(1, 69, false);

        user.ReadChapter(1);

        // Act
        MissionService.CompleteMission(user, mission);

        // Assert
        user.Ledgers.Should().BeEmpty();
        user.KanaCoin.Should().Be(0);
    }
}
