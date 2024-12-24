namespace DashiToon.Api.Application.Homepage.Queries.GetRecommendSeries;

public class UserLikedSeriesDto
{
    public required string UserId { get; set; }
    public required int[] SeriesArray { get; set; }
}
