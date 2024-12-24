using DashiToon.Api.Application.Genres.Queries.GetGenres;

namespace Application.IntegrationTests.Genres.Queries;

using static Testing;

[Collection("Serialize")]
public class GetGenresTest : BaseIntegrationTest
{
    public GetGenresTest(Testing testing) : base(testing)
    {
        ResetState().Wait();
    }

    [Fact]
    public async Task GetGenresShouldReturnAllGenres()
    {
        // Act
        List<GenreVm>? genres = await SendAsync(new GetGenresQuery());

        // Arrange
        genres.Should().NotBeNull();
    }
}
