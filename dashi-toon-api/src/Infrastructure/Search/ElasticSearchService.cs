using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Search.Models;
using DashiToon.Api.Domain.Entities;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Analysis;
using Elastic.Clients.Elasticsearch.IndexManagement;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ExistsResponse = Elastic.Clients.Elasticsearch.IndexManagement.ExistsResponse;

namespace DashiToon.Api.Infrastructure.Search;

public class ElasticSearchService : ISearchService
{
    private readonly ElasticsearchClient _client;
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<ElasticSearchService> _logger;
    private const string IndexName = "dashi-toon-series-index";

    public ElasticSearchService(
        ElasticsearchClient client,
        IApplicationDbContext dbContext,
        ILogger<ElasticSearchService> logger)
    {
        _client = client;
        _dbContext = dbContext;
        _logger = logger;
    }

    public static async Task<ElasticSearchService> CreateAsync(
        ElasticsearchClient client,
        IApplicationDbContext dbContext,
        ILogger<ElasticSearchService> logger)
    {
        ElasticSearchService? service = new(client, dbContext, logger);
        await service.InitializeAsync();
        return service;
    }

    private async Task InitializeAsync()
    {
        _logger.LogInformation("Initializing Elasticsearch for index: {IndexName}", IndexName);

        ExistsResponse? existsResponse = await _client.Indices.ExistsAsync(IndexName);
        if (!existsResponse.Exists)
        {
            await CreateIndexAsync();
        }
    }

    private async Task CreateIndexAsync()
    {
        _logger.LogInformation("Creating Elasticsearch index: {IndexName}", IndexName);

        CreateIndexResponse? newIndexResponse = await _client.Indices.CreateAsync(IndexName, descriptor => descriptor
            .Settings(s => s
                .Analysis(a => a
                    .TokenFilters(tf => tf
                        .EdgeNGram("autocomplete_filter", eng => eng
                            .MinGram(1)
                            .MaxGram(20)))
                    .Analyzers(anal => anal
                        .Custom("autocomplete", c => c
                            .Tokenizer("standard")
                            .Filter(["lowercase", "autocomplete_filter"])))))
            .Mappings(m => m
                .Properties<SearchModel>(p => p
                    .IntegerNumber(i => i.Id)
                    .Text(t => t.Thumbnail ?? string.Empty)
                    .Text(t => t.Title, tf => tf
                        .Analyzer("autocomplete")
                        .SearchAnalyzer("standard")
                        .Fields(f => f.Keyword("keyword")))
                    .Keyword(k => k.Type)
                    .Keyword(k => k.Status)
                    .Keyword(k => k.ContentRating)
                    .Text(t => t.AlternativeTitles)
                    .Text(t => t.Authors)
                    .Keyword(k => k.Genres)
                    .Date(d => d.StartTime ?? default)
                    .IntegerNumber(i => i.ViewCount)
                    .DoubleNumber(d => d.Rating)
                )
            )
        );

        if (!newIndexResponse.IsValidResponse || !newIndexResponse.Acknowledged)
        {
            _logger.LogError("Index creation failed: {Reason}", newIndexResponse.DebugInformation);
            throw new Exception($"Unable to create index: {newIndexResponse.DebugInformation}");
        }

        List<SearchModel>? series = await _dbContext.Series
            .AsNoTracking()
            .Include(s => s.Genres)
            .Include(s => s.Reviews)
            .Include(s => s.Volumes)
            .ThenInclude(v => v.Chapters)
            .AsSplitQuery()
            .Select(s => SearchModel.FromEntity(
                s,
                s.Volumes.SelectMany(v => v.Chapters).Sum(c => c.ViewCount),
                s.Reviews.Count == 0 ? 0 : (double)s.Reviews.Count(r => r.IsRecommended) * 100 / s.Reviews.Count))
            .ToListAsync();

        IndexSeries(series);
    }

    private void IndexSeries(IEnumerable<SearchModel> series)
    {
        BulkAllObservable<SearchModel>? bulkAll = _client.BulkAll(series, cfg => cfg
            .Index(IndexName)
            .BackOffRetries(2)
            .BackOffTime(TimeSpan.FromMinutes(5))
            .ContinueAfterDroppedDocuments()
            .DroppedDocumentCallback((r, s) =>
            {
                _logger.LogError("Indexing failed for document with error: {Error}. Document: {Document}",
                    r.Error?.Reason, s);
            })
            .MaxDegreeOfParallelism(2)
            .Size(500)
        );

        bulkAll.Wait(
            TimeSpan.FromMinutes(5),
            resp => _logger.LogInformation("Successfully indexed {ItemCount} items", resp.Items.Count));
    }

    public async Task<bool> IndexSeriesAsync(Series series)
    {
        SearchModel? document = SearchModel.FromEntity(series);
        IndexResponse? response = await _client.IndexAsync(document, idx => idx.Index(IndexName));

        if (!response.IsValidResponse)
        {
            _logger.LogError("Failed to index series {SeriesId}: {Error}",
                series.Id, response.DebugInformation);
        }

        return response.IsValidResponse;
    }

    public async Task<bool> UpdateSeriesAsync(Series series)
    {
        SearchModel? document = SearchModel.FromEntity(series);
        UpdateResponse<SearchModel>? response = await _client.UpdateAsync<SearchModel, SearchModel>(
            IndexName,
            document.Id,
            u => u.Doc(document)
        );

        if (!response.IsValidResponse)
        {
            _logger.LogError("Failed to update series {SeriesId}: {Error}",
                series.Id, response.DebugInformation);
        }

        return response.IsValidResponse || response.Result == Result.NotFound;
    }

    public async Task<bool> BulkUpdateViewCountAsync(Dictionary<int, int> seriesViewCount)
    {
        BulkResponse? bulkResponse = await _client.BulkAsync(b => b
            .Index(IndexName)
            .UpdateMany(seriesViewCount.Select(s => new { s.Key, s.Value }), (descriptor, viewCountUpdates) =>
                descriptor
                    .Id(viewCountUpdates.Key)
                    .Script(s => s
                        .Source("ctx._source.viewCount += params.increment")
                        .Params(p => p
                            .Add("increment", viewCountUpdates.Value)))));

        if (!bulkResponse.IsValidResponse)
        {
            _logger.LogError("Failed to update view count: {Error}", bulkResponse.DebugInformation);
        }

        _logger.LogInformation(bulkResponse.DebugInformation);

        return bulkResponse.IsValidResponse;
    }

    public async Task<bool> UpdateSeriesRatingAsync(int seriesId, double rating)
    {
        UpdateResponse<SearchModel>? updateResponse = await _client.UpdateAsync<SearchModel, SeriesRatingModel>(
            IndexName, seriesId, u => u
                .Doc(new SeriesRatingModel { Rating = rating })
        );

        if (!updateResponse.IsValidResponse)
        {
            _logger.LogError("Failed to update rating: {Error}", updateResponse.DebugInformation);
        }

        _logger.LogInformation(updateResponse.DebugInformation);

        return updateResponse.IsValidResponse;
    }

    public class SeriesRatingModel
    {
        public double Rating { get; set; }
    }

    public async Task<bool> DeleteSeriesAsync(int id)
    {
        DeleteResponse? response = await _client.DeleteAsync<SearchModel>(
            IndexName,
            id.ToString()
        );

        if (!response.IsValidResponse && response.Result != Result.NotFound)
        {
            _logger.LogError("Failed to delete series {SeriesId}: {Error}",
                id, response.DebugInformation);
        }

        return response.IsValidResponse || response.Result == Result.NotFound;
    }

    public async Task<SeriesSearchResult> SearchSeriesAsync(SeriesSearchOptions options)
    {
        SearchRequestDescriptor<SearchModel>? searchRequest = new SearchRequestDescriptor<SearchModel>()
            .Index(IndexName)
            .From((options.Page - 1) * options.PageSize)
            .Size(options.PageSize)
            .Query(q => BuildQuery(q, options));

        SortOrder sortOder = options.SortOrder.Equals("asc", StringComparison.CurrentCultureIgnoreCase)
            ? SortOrder.Asc
            : SortOrder.Desc;

        switch (options.SortBy)
        {
            case "trending":
                searchRequest.Sort(so => so
                    .Field(f => f.ViewCount, new FieldSort { Order = sortOder }));
                break;
            case "title":
                searchRequest.Sort(so => so
                    .Field(f => f.Title.Suffix("keyword"), new FieldSort { Order = sortOder }));
                break;
            case "rating":
                searchRequest.Sort(so => so
                    .Field(f => f.Rating, new FieldSort { Order = sortOder }));
                break;
        }

        SearchResponse<SearchModel>? searchResponse = await _client.SearchAsync(searchRequest);

        if (!searchResponse.IsValidResponse)
        {
            throw new Exception($"Search failed: {searchResponse.DebugInformation}");
        }

        _logger.LogInformation(searchResponse.DebugInformation);

        return new SeriesSearchResult { Items = searchResponse.Documents, TotalCount = searchResponse.Total };
    }

    private static void BuildQuery(QueryDescriptor<SearchModel> q, SeriesSearchOptions options)
    {
        q.Bool(b =>
        {
            if (!string.IsNullOrEmpty(options.SearchTerm))
            {
                b.Must(must => must
                    .MultiMatch(mm => mm
                        .Fields(Fields.FromFields([
                            new Field("title"),
                            new Field("alternativeTitles"),
                            new Field("authors")
                        ]))
                        .Query(options.SearchTerm)
                        .Operator(Operator.And))
                );
            }

            if (options.Types.Length != 0 || options.ContentRatings.Length != 0 || options.Statuses.Length != 0 ||
                options.Genres.Length != 0)
            {
                List<Action<QueryDescriptor<SearchModel>>>? queries = new();

                if (options.Types.Length != 0)
                {
                    queries.Add(filter => filter.Terms(t => t
                        .Field(f => f.Type)
                        .Terms(new TermsQueryField(options.Types.Select(st => (FieldValue)st.ToString())
                            .ToList()))));
                }

                if (options.ContentRatings.Length != 0)
                {
                    queries.Add(filter => filter.Terms(t => t
                        .Field(f => f.ContentRating)
                        .Terms(new TermsQueryField(options.ContentRatings.Select(cr => (FieldValue)cr.ToString())
                            .ToList()))));
                }

                if (options.Statuses.Length != 0)
                {
                    queries.Add(filter => filter.Terms(t => t
                        .Field(f => f.Status)
                        .Terms(new TermsQueryField(options.Statuses.Select(ss => (FieldValue)ss.ToString())
                            .ToList()))));
                }

                if (options.Genres.Length != 0)
                {
                    List<FieldValue>? values = options.Genres.Select(genre => (FieldValue)genre).ToList();

                    queries.Add(filter => filter.Terms(t => t
                        .Field(g => g.Genres)
                        .Terms(new TermsQueryField(values))));
                }

                b.Filter(queries.ToArray());
            }
        });
    }
}
