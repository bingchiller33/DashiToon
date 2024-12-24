using System.Data.Common;

namespace Application.IntegrationTests;

public interface ITestDatabase
{
    Task InitializeAsync();

    DbConnection GetConnection();

    Task ResetAsync();

    Task DisposeAsync();
}
