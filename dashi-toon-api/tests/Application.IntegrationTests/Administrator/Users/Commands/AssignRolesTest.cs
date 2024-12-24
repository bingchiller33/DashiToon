using DashiToon.Api.Application.Administrator.Users.Commands.AssignRole;
using DashiToon.Api.Application.Administrator.Users.Models;
using DashiToon.Api.Application.Administrator.Users.Queries.GetUsers;
using DashiToon.Api.Application.Common.Models;
using DashiToon.Api.Domain.Constants;

namespace Application.IntegrationTests.Administrator.Users.Commands;

using static Testing;

[Collection("Serialize")]
public class AssignRolesTest : BaseIntegrationTest
{
    public AssignRolesTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task AssignRoleShouldUpdateUserRoles()
    {
        // Arrange
        string? user1Id = await RunAsUserAsync("TestUser1", "Bruh@1311", []);

        await RunAsAdministratorAsync();

        // Act
        await SendAsync(new AssignRoleCommand(user1Id, Roles.Moderator));

        // Assert
        PaginatedList<UserVm>? users = await SendAsync(new GetUsersQuery(user1Id, null, null));

        users.TotalCount.Should().Be(1);
        users.Items.First().Roles.Should().Contain(Roles.Moderator);
    }
}
