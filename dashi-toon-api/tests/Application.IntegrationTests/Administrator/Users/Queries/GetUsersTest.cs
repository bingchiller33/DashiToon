using DashiToon.Api.Application.Administrator.Users.Models;
using DashiToon.Api.Application.Administrator.Users.Queries.GetUsers;
using DashiToon.Api.Application.Common.Models;
using DashiToon.Api.Domain.Constants;

namespace Application.IntegrationTests.Administrator.Users.Queries;

using static Testing;

[Collection("Serialize")]
public class GetUsersTest : BaseIntegrationTest
{
    public GetUsersTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task GetUsersShouldReturnUsers()
    {
        // Arrange
        string? user1Id = await RunAsUserAsync("User1", "Bruh@1311", []);
        string? user2Id = await RunAsUserAsync("User2", "Bruh@1311", []);
        string? user3Id = await RunAsUserAsync("User3", "Bruh@1311", []);
        string? user4Id = await RunAsUserAsync("User4", "Bruh@1311", []);

        string? modId = await RunAsModeratorAsync();

        string? adminId = await RunAsAdministratorAsync();

        List<UserVm>? expected = new()
        {
            new UserVm(user1Id, "User1", "User1", null, []),
            new UserVm(user2Id, "User2", "User2", null, []),
            new UserVm(user3Id, "User3", "User3", null, []),
            new UserVm(user4Id, "User4", "User4", null, []),
            new UserVm(modId, "moderator@local", "moderator@local", null, ["Moderator"]),
            new UserVm(adminId, "administrator@local", "administrator@local", null, ["Administrator"])
        };

        // Act
        PaginatedList<UserVm>? users = await SendAsync(new GetUsersQuery(null, null, null));

        // Assert
        users.TotalCount.Should().Be(6);
        users.Items.Should().BeEquivalentTo(expected);
    }
}
