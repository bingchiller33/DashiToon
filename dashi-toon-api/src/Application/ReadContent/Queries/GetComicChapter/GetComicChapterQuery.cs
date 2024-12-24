using DashiToon.Api.Application.AuthorStudio.Chapters.Models;
using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Services;
using DashiToon.Api.Domain.Constants;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Enums;
using DashiToon.Api.Domain.Events;
using DashiToon.Api.Domain.Services;
using FluentValidation.Results;
using ValidationException = DashiToon.Api.Application.Common.Exceptions.ValidationException;

namespace DashiToon.Api.Application.ReadContent.Queries.GetComicChapter;

public sealed record GetComicChapterQuery(int SeriesId, int VolumeId, int ChapterId) : IRequest<ComicChapterDetailVm>;

public sealed class GetComicChapterQueryHandler : IRequestHandler<GetComicChapterQuery, ComicChapterDetailVm>
{
    private readonly ISeriesRepository _seriesRepository;
    private readonly IUserRepository _userRepository;
    private readonly ComicChapterService _comicChapterService;
    private readonly IIdentityService _identityService;
    private readonly IImageStore _imageStore;
    private readonly IPublisher _publisher;
    private readonly IUser _user;

    public GetComicChapterQueryHandler(
        ISeriesRepository seriesRepository,
        IUserRepository userRepository,
        ComicChapterService comicChapterService,
        IIdentityService identityService,
        IImageStore imageStore,
        IPublisher publisher, IUser user)
    {
        _seriesRepository = seriesRepository;
        _userRepository = userRepository;
        _comicChapterService = comicChapterService;
        _identityService = identityService;
        _imageStore = imageStore;
        _publisher = publisher;
        _user = user;
    }


    public async Task<ComicChapterDetailVm> Handle(GetComicChapterQuery request, CancellationToken cancellationToken)
    {
        Series? series = await _seriesRepository.FindSeriesWithVolumesAndChaptersById(request.SeriesId);

        if (series is null)
        {
            throw new NotFoundException(request.SeriesId.ToString(), nameof(Series));
        }

        if (series.Type is not SeriesType.Comic)
        {
            throw new ValidationException([
                new ValidationFailure(nameof(Series.Type), "Must of type comic")
            ]);
        }

        IReadOnlyList<Volume>? volumes = series.Volumes;

        Volume? volume = volumes.FirstOrDefault(v => v.Id == request.VolumeId);

        if (volume is null)
        {
            throw new NotFoundException(request.VolumeId.ToString(), nameof(Volume));
        }

        List<Chapter>? publishedChapters = volumes
            .SelectMany(v => v.Chapters.Where(c => c.GetPublishedVersion() != null))
            .ToList();

        if (publishedChapters.All(c => c.Id != request.ChapterId))
        {
            throw new NotFoundException(request.ChapterId.ToString(), nameof(Chapter));
        }

        ChapterService? chapterService = new();

        if (!chapterService.IsGuestUserAllowedToReadChapter(publishedChapters, request.ChapterId))
        {
            IDomainUser? user = await _userRepository.GetById(_user.Id ?? string.Empty);

            if (user is null)
            {
                throw new UnauthorizedAccessException();
            }

            if (await _identityService.IsInOneOfRolesAsync(user.Id, Roles.Administrator, Roles.Moderator) != true
                && series.CreatedBy != user.Id)
            {
                if (!chapterService.IsUserAllowedToReadChapter(user, series.Id, publishedChapters, request.ChapterId))
                {
                    throw new ForbiddenAccessException();
                }
            }
        }

        if (_user.Id != null)
        {
            await _publisher.Publish(new ChapterReadEvent(request.ChapterId), cancellationToken);
        }

        Chapter? chapter = publishedChapters.First(c => c.Id == request.ChapterId);

        ChapterVersion? publishedVersion = chapter.GetPublishedVersion()!;

        List<ImageInfo> content = await _comicChapterService.ProcessContent(publishedVersion.Content);

        return new ComicChapterDetailVm(
            chapter.Id,
            chapter.ChapterNumber,
            publishedVersion.Title,
            string.IsNullOrEmpty(publishedVersion.Thumbnail)
                ? await _imageStore.GetUrl("thumbnails/default.png", DateTime.UtcNow.AddMinutes(2))
                : await _imageStore.GetUrl($"thumbnails/{publishedVersion.Thumbnail}", DateTime.UtcNow.AddMinutes(2)),
            chapter.KanaPrice,
            chapter.IsAdvanceChapter,
            content,
            publishedVersion.Status,
            chapter.PublishedDate?.ToString("O")
        );
    }
}
