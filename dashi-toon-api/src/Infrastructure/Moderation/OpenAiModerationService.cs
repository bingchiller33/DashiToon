using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DashiToon.Api.Infrastructure.Moderation;

public class OpenAiModerationService : IModerationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OpenAiModerationService> _logger;

    public OpenAiModerationService(
        HttpClient httpClient,
        IOptions<OpenAiOptions> options,
        ILogger<OpenAiModerationService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _httpClient.BaseAddress = new Uri("https://api.openai.com");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.Value.ApiKey);
    }

    public async Task<ModerationAnalysis> ModerateComment(string comment)
    {
        ModerationRequest? request = new();
        request.Input.Add(new TextContent(comment));

        return await AnalyzeContent(request);
    }

    public async Task<ModerationAnalysis> ModerateReview(string review)
    {
        ModerationRequest? request = new();
        request.Input.Add(new TextContent(review));

        return await AnalyzeContent(request);
    }

    public async Task<ModerationAnalysis> ModerateComicChapter(List<string> images)
    {
        List<Task<ModerationAnalysis>>? tasks = new();

        foreach (string? image in images)
        {
            ModerationRequest? request = new();

            request.Input.Add(new ImageContent(image));

            tasks.Add(AnalyzeContent(request));
        }

        ModerationAnalysis[]? results = await Task.WhenAll(tasks);

        return AggregateAnalyses(results);
    }

    public async Task<ModerationAnalysis> ModerateNovelChapter(List<string> images, string text)
    {
        List<Task<ModerationAnalysis>>? tasks = new();

        foreach (string? image in images)
        {
            ModerationRequest? request = new();

            request.Input.Add(new ImageContent(image));

            tasks.Add(AnalyzeContent(request));
        }

        tasks.Add(AnalyzeContent(new ModerationRequest { Input = [new TextContent(text)] }));

        ModerationAnalysis[]? results = await Task.WhenAll(tasks);

        return AggregateAnalyses(results);
    }

    private static ModerationAnalysis AggregateAnalyses(ModerationAnalysis[] moderationAnalyses)
    {
        bool isFlagged = false;

        Dictionary<string, float>? categoryScores = new();

        foreach (ModerationAnalysis? result in moderationAnalyses)
        {
            if (!result.Flagged)
            {
                continue;
            }

            isFlagged = true;

            foreach (CategoryScore? categoryScore in result.FlaggedCategories)
            {
                if (categoryScores.TryGetValue(categoryScore.Category, out float categoryScoreValue))
                {
                    if (categoryScore.Score > categoryScoreValue)
                    {
                        categoryScores[categoryScore.Category] = categoryScore.Score;
                    }
                }
                else
                {
                    categoryScores.Add(categoryScore.Category, categoryScore.Score);
                }
            }
        }

        return ModerationAnalysis.Create(
            isFlagged,
            categoryScores
                .Select(score => new CategoryScore { Category = score.Key, Score = score.Value }).ToList());
    }

    public async Task<ModerationAnalysis> ModerateSeries(string title, string synopsis, string? thumbnailUrl)
    {
        ModerationRequest? request = new();

        request.Input.Add(new TextContent(title));


        if (thumbnailUrl is not null)
        {
            request.Input.Add(new ImageContent(thumbnailUrl));
        }

        request.Input.Add(new TextContent(title));
        request.Input.Add(new TextContent(synopsis));

        return await AnalyzeContent(request);
    }

    private async Task<ModerationAnalysis> AnalyzeContent(ModerationRequest request)
    {
        request.Model = "omni-moderation-latest";

        _logger.LogInformation(
            "Request content: {Request}",
            JsonSerializer.Serialize(
                request,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower }));

        HttpResponseMessage? response = await _httpClient.PostAsJsonAsync(
            "v1/moderations",
            request,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower });

        _logger.LogInformation("Response status code: {StatusCode}", (int)response.StatusCode);
        _logger.LogInformation("Response headers: {Headers}", response.Headers);
        _logger.LogInformation("Response content: {Response}", await response.Content.ReadAsStringAsync());

        response.EnsureSuccessStatusCode();

        ModerationResponse? moderationResponse = await response.Content.ReadFromJsonAsync<ModerationResponse>(
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower });

        ModerationResult? result = moderationResponse!.Results.First();

        List<CategoryScore>? flaggedCategories = result.Categories
            .Where(c => c.Value)
            .Select(c => new CategoryScore { Category = c.Key, Score = result.CategoryScores.GetValueOrDefault(c.Key) })
            .ToList();

        return ModerationAnalysis.Create(
            result.Flagged,
            flaggedCategories
        );
    }
}
