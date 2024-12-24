using DashiToon.Api.Domain.Enums;

namespace DashiToon.Api.Application.Administrator.Series.ExportSeries;

public class ExportSeriesVm
{
    public string Title { get; set; } = null!;
    public string Synopsis { get; set; } = null!;
    public string? Thumbnail { get; set; }
    public SeriesType Type { get; set; }
    public SeriesStatus Status { get; set; }
    public string[] AlternativeTitles { get; set; } = [];
    public string[] Authors { get; set; } = [];
    public int VolumeCount { get; set; }
    public DateTimeOffset? StartTime { get; set; }

    public List<string> Genres { get; set; } = [];
    public List<ExportCategoryRatingVm> CategoryRatings { get; set; } = [];
    public List<ExportVolumeVm> Volumes { get; set; } = [];
}

public class ExportCategoryRatingVm
{
    public ContentCategory Category { get; set; }
    public int Rating { get; set; }
}

public class ExportVolumeVm
{
    public string Name { get; set; } = null!;
    public string? Introduction { get; set; }
    public int VolumeNumber { get; set; }

    public List<ExportChapterVm> Chapters { get; set; } = [];
}

public class ExportChapterVm
{
    public int ChapterNumber { get; set; }
    public string Title { get; set; } = null!;
    public string? Thumbnail { get; set; }
    public string Content { get; set; } = null!;
    public string? Note { get; set; }
}
