using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Enums;
using DashiToon.Api.Domain.Services;

namespace DashiToon.Api.Application.ReadContent.Commands.UnlockChapter;

[Authorize]
public sealed record UnlockChapterCommand(int SeriesId, int VolumeId, int ChapterId) : IRequest;

public sealed class UnlockChapterCommandHandler : IRequestHandler<UnlockChapterCommand>
{
    private readonly ChapterService _chapterService;
    private readonly RevenueService _revenueService;
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserRepository _userRepository;
    private readonly IUser _user;

    public UnlockChapterCommandHandler(
        IApplicationDbContext dbContext,
        IUserRepository userRepository,
        IUser user)
    {
        _chapterService = new ChapterService();
        _revenueService = new RevenueService();
        _dbContext = dbContext;
        _userRepository = userRepository;
        _user = user;
    }

    public async Task Handle(UnlockChapterCommand request, CancellationToken cancellationToken)
    {
        IDomainUser? user = await _userRepository.GetById(_user.Id!);

        if (user is null)
        {
            throw new UnauthorizedAccessException();
        }

        Series? series = await _dbContext.Series
            .Include(s => s.Volumes.Where(v => v.Id == request.VolumeId))
            .FirstOrDefaultAsync(s =>
                    s.Id == request.SeriesId
                    && s.Status != SeriesStatus.Hiatus
                    && s.Status != SeriesStatus.Draft
                    && s.Status != SeriesStatus.Trashed,
                cancellationToken);

        if (series is null)
        {
            throw new NotFoundException(request.SeriesId.ToString(), nameof(Series));
        }

        if (!series.Volumes.Any())
        {
            throw new NotFoundException(request.VolumeId.ToString(), nameof(Volume));
        }

        Chapter? chapter = await _dbContext.Chapters
            .Include(c => c.Volume)
            .ThenInclude(v => v.Series)
            .FirstOrDefaultAsync(c =>
                c.Id == request.ChapterId
                && c.VolumeId == request.VolumeId
                && c.PublishedVersionId != null, cancellationToken);

        if (chapter is null)
        {
            throw new NotFoundException(request.ChapterId.ToString(), nameof(Chapter));
        }

        _chapterService.UnlockChapter(user, chapter);

        IDomainUser? author = await _userRepository.GetById(series.CreatedBy!);
        CommissionRate? commissionRate = await _dbContext.CommissionRates
            .FirstAsync(cr => cr.Type == CommissionType.Kana, cancellationToken);
        KanaExchangeRate? exchangeRate = await _dbContext.KanaExchangeRates.FirstAsync(cancellationToken);

        if (author is not null)
        {
            _revenueService.ReceiveKanaRevenue(author, chapter, commissionRate, exchangeRate);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
