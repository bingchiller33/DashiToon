using DashiToon.Api.Domain.Enums;

namespace DashiToon.Api.Application.Search.Models;

public class SeriesSearchOptions
{
    public string? SearchTerm { get; init; }
    public SeriesType[] Types { get; init; } = [];
    public SeriesStatus[] Statuses { get; init; } = [];
    public ContentRating[] ContentRatings { get; init; } = [];
    public string[] Genres { get; init; } = [];
    public int Page { get; init; }
    public int PageSize { get; init; }
    public required string SortBy { get; init; }
    public required string SortOrder { get; init; }
}
