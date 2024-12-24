using DashiToon.Api.Application.Administrator.Users.Queries.GetRoles;
using DashiToon.Api.Domain.Constants;

namespace Application.IntegrationTests.Administrator.Users.Queries;

using static Testing;

[Collection("Serialize")]
public class GetRolesTest : BaseIntegrationTest
{
    public GetRolesTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task GetRolesShouldReturnAllRoles()
    {
        // Arrange
        await RunAsAdministratorAsync();

        string[]? expectedRoles = new[] { "ADMINISTRATOR", "MODERATOR" };

        // Act
        List<string>? roles = await SendAsync(new GetRolesQuery());

        // Assert
        roles.Should().HaveCount(2);
        roles.Should().BeEquivalentTo(expectedRoles);
    }
}
