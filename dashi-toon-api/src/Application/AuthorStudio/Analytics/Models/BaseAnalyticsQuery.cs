namespace DashiToon.Api.Application.AuthorStudio.Analytics.Models;

public record BaseAnalyticsQuery(DateRange Current, DateRange? Compare, int SeriesId);
