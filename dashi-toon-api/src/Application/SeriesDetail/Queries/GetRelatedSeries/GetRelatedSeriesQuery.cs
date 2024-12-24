using DashiToon.Api.Application.Common.Interfaces;

namespace DashiToon.Api.Application.SeriesDetail.Queries.GetRelatedSeries;

public sealed record GetRelatedSeriesQuery(int SeriesId) : IRequest<List<RelatedSeriesVm>>;

public sealed class GetRelatedSeriesQueryHandler : IRequestHandler<GetRelatedSeriesQuery, List<RelatedSeriesVm>>
{
    private readonly IImageStore _imageStore;
    private readonly ISeriesRepository _seriesRepository;

    public GetRelatedSeriesQueryHandler(ISeriesRepository seriesRepository, IImageStore imageStore)
    {
        _seriesRepository = seriesRepository;
        _imageStore = imageStore;
    }

    public async Task<List<RelatedSeriesVm>> Handle(GetRelatedSeriesQuery request,
        CancellationToken cancellationToken)
    {
        List<RelatedSeriesDto>? relatedSeries = (await _seriesRepository.GetRelatedSeries(request.SeriesId)).ToList();

        RelatedSeriesDto? currentSeries = relatedSeries.FirstOrDefault();
        relatedSeries = relatedSeries.Skip(1).ToList();

        List<RelatedSeriesVm>? result = new();

        if (currentSeries == null)
        {
            return result;
        }

        List<(RelatedSeriesVm Series, double SimilarityScore)>? similarityScores = new();

        foreach (RelatedSeriesDto item in relatedSeries)
        {
            RelatedSeriesVm series = new(
                item.Id,
                item.Title,
                item.AlternativeTitles,
                item.Authors,
                item.Rating,
                item.Status,
                string.IsNullOrEmpty(item.Thumbnail)
                    ? await _imageStore.GetUrl("thumbnails/default.png", DateTime.UtcNow.AddMinutes(2))
                    : await _imageStore.GetUrl($"thumbnails/{item.Thumbnail}", DateTime.UtcNow.AddMinutes(2)),
                item.Type,
                item.Genres,
                item.ContentRating
            );

            double genreSimilarity = ComputeGenreSimilarity(currentSeries, item);
            double yearSimilarity = ComputeYearSimilarity(currentSeries, item);
            double similarityScore = genreSimilarity * yearSimilarity;
            similarityScores.Add((series, similarityScore));
        }

        similarityScores = similarityScores
            .OrderByDescending(x => x.SimilarityScore)
            .Take(12)
            .ToList();

        return similarityScores.Select(x => x.Series).ToList();
    }

    private double ComputeGenreSimilarity(RelatedSeriesDto series1, RelatedSeriesDto series2)
    {
        int[] genres1 = series1.GenresMap;
        int[] genres2 = series2.GenresMap;

        double sumxx = 0, sumxy = 0, sumyy = 0;

        for (int i = 0; i < genres1.Length; i++)
        {
            int x = genres1[i];
            int y = genres2[i];

            sumxx += x * x;
            sumyy += y * y;
            sumxy += x * y;
        }

        return sumxy / Math.Sqrt(sumxx * sumyy);
    }

    private double ComputeYearSimilarity(RelatedSeriesDto series1, RelatedSeriesDto series2)
    {
        int diff = Math.Abs(series1.StartTime?.Year ?? 0 - series2.StartTime?.Year ?? 0);
        double sim = Math.Exp(-diff / 10.0);
        return sim;
    }
}
