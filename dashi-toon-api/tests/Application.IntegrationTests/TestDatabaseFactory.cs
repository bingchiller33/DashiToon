namespace Application.IntegrationTests;

public static class TestDatabaseFactory
{
    public static async Task<ITestDatabase> CreateAsync()
    {
        TestContainersTestDatabase database = new();

        await database.InitializeAsync();

        return database;
    }
}
