using DashiToon.Api.Application.AuthorStudio.Chapters.Models;
using DashiToon.Api.Application.ReadContent.Queries.Models;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Enums;

namespace DashiToon.Api.Application.Common.Interfaces;

public interface IChapterRepository
{
    Task<Chapter?> FindChapterById(int chapterId);

    Task<(int Count, IEnumerable<ChapterInfo> Chapters)> FindChapters(
        string? title,
        ChapterStatus? status,
        int volumeId,
        int pageNumber = 1,
        int pageSize = 10
    );

    Task<IEnumerable<ChapterInfo>> FindPublishedChaptersByVolumeId(int volumeId);
    Task<ChapterContent?> FindPublishedChapterById(int volumeId, int chapterId);

    Task<(int Count, IEnumerable<ChapterVersion> Versions)> FindChapterVersions(
        int id,
        string versionName,
        DateTimeOffset? from,
        DateTimeOffset? to,
        bool includeAutoSave = false,
        int pageNumber = 1,
        int pageSize = 10);

    Task<IEnumerable<int>> FindUserUnlockedChapterIds(string? userId);
}
