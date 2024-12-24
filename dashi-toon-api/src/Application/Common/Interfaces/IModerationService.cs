using DashiToon.Api.Domain.Entities;

namespace DashiToon.Api.Application.Common.Interfaces;

public interface IModerationService
{
    Task<ModerationAnalysis> ModerateComment(string comment);
    Task<ModerationAnalysis> ModerateReview(string review);
    Task<ModerationAnalysis> ModerateComicChapter(List<string> images);
    Task<ModerationAnalysis> ModerateNovelChapter(List<string> images, string text);
    Task<ModerationAnalysis> ModerateSeries(string title, string synopsis, string? thumbnailUrl);
}
