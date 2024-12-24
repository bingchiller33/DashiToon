namespace DashiToon.Api.Application.Genres.Queries.GetGenres;

public record GenreVm(int Id, string Name);

public sealed class GenreDto
{
    public int Id { get; init; }
    public required string Name { get; init; }
}
