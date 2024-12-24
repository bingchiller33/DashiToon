using System.Text.Json;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Domain.Constants;

namespace DashiToon.Api.Application.Administrator.Series.ExportSeries;

[Authorize(Roles = Roles.Administrator)]
public sealed record ExportSeriesCommand : IRequest<byte[]>;

public sealed class ExportSeriesCommandHandler : IRequestHandler<ExportSeriesCommand, byte[]>
{
    private readonly IApplicationDbContext _context;

    public ExportSeriesCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<byte[]> Handle(ExportSeriesCommand request, CancellationToken cancellationToken)
    {
        List<Domain.Entities.Series>? series = await _context.Series
            .Include(s => s.Genres)
            .Include(s => s.Volumes)
            .ThenInclude(s => s.Chapters)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        IEnumerable<ExportSeriesVm>? exportedSeries = series.Select(s => new ExportSeriesVm
        {
            Title = s.Title,
            Synopsis = s.Synopsis,
            Thumbnail = s.Thumbnail,
            Type = s.Type,
            Status = s.Status,
            AlternativeTitles = s.AlternativeTitles,
            Authors = s.Authors,
            VolumeCount = s.VolumeCount,
            StartTime = s.StartTime,
            Genres = s.Genres.Select(g => g.Name).ToList(), // Assuming Genre has a Name property
            CategoryRatings = s.CategoryRatings
                .Select(cr => new ExportCategoryRatingVm { Category = cr.Category, Rating = cr.Rating })
                .ToList(),
            Volumes = s.Volumes
                .Select(v => new ExportVolumeVm
                {
                    Name = v.Name,
                    Introduction = v.Introduction,
                    VolumeNumber = v.VolumeNumber,
                    Chapters = v.Chapters
                        .Where(c => c.PublishedVersionId != null)
                        .Select(c => new ExportChapterVm
                        {
                            ChapterNumber = c.ChapterNumber,
                            Title = c.GetPublishedVersion()!.Title,
                            Thumbnail = c.GetPublishedVersion()!.Thumbnail,
                            Content = c.GetPublishedVersion()!.Content,
                            Note = c.GetPublishedVersion()!.Note
                        })
                        .ToList()
                })
                .ToList()
        });

        JsonSerializerOptions? jsonOptions = new()
        {
            WriteIndented = true // to format JSON in a readable way
        };

        return JsonSerializer.SerializeToUtf8Bytes(exportedSeries, jsonOptions);
    }
}
