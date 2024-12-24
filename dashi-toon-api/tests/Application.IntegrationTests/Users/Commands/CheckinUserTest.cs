using DashiToon.Api.Application.Common.Models;
using DashiToon.Api.Application.Users.Commands.CheckinUser;
using DashiToon.Api.Application.Users.Queries.GetKanaTotals;
using DashiToon.Api.Application.Users.Queries.GetKanaTransactions;
using DashiToon.Api.Application.Users.Queries.GetUserMetadatas;
using DashiToon.Api.Domain.Enums;

namespace Application.IntegrationTests.Users.Commands;

using static Testing;

[Collection("Serialize")]
public class CheckinUserTest : BaseIntegrationTest
{
    public CheckinUserTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task CheckinUserShouldTopUpKanaCoin()
    {
        // Arrange
        await RunAsDefaultUserAsync();

        // Act
        await SendAsync(new CheckinUserCommand());

        // Assert
        KanaTotalsVm? kanaTotals = await SendAsync(new GetKanaTotalsQuery());
        UserMetadata? userMetadata = await SendAsync(new GetUserMetadataQuery());
        PaginatedList<KanaTransactionVm>? kanaTransactions = await SendAsync(new GetKanaTransactionsQuery("EARN"));

        kanaTotals.Totals[0].Amount.Should().Be(100);
        userMetadata.IsCheckedIn.Should().BeTrue();
        kanaTransactions.TotalCount.Should().Be(1);
        kanaTransactions.Items.First().Type.Should().Be(TransactionType.Checkin);
        kanaTransactions.Items.First().Amount.Should().Be(100);
        kanaTransactions.Items.First().Currency.Should().Be(KanaType.Coin);
    }
}
