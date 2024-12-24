using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Domain.Constants;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Events;
using DashiToon.Api.Domain.Services;

namespace DashiToon.Api.Application.Moderation.Commands.ResolveChapterReport;

[Authorize(Roles = Roles.Moderator)]
public sealed record ResolveChapterReportCommand(int ChapterId, int RestrictedDurationInDays) : IRequest;

public sealed class ResolveChapterReportCommandHandler : IRequestHandler<ResolveChapterReportCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserRepository _userRepository;
    private readonly IPublisher _publisher;

    public ResolveChapterReportCommandHandler(IApplicationDbContext context, IUserRepository userRepository,
        IPublisher publisher)
    {
        _context = context;
        _userRepository = userRepository;
        _publisher = publisher;
    }

    public async Task Handle(ResolveChapterReportCommand request, CancellationToken cancellationToken)
    {
        Chapter? chapter = await _context.Chapters
            .Include(c => c.Volume)
            .ThenInclude(v => v.Series)
            .FirstOrDefaultAsync(c => c.Id == request.ChapterId, cancellationToken);

        if (chapter is null)
        {
            throw new NotFoundException(request.ChapterId.ToString(), nameof(Chapter));
        }

        chapter.Unpublish();

        List<Report>? reports = await _context.Reports
            .Where(c => c.ChapterId == request.ChapterId)
            .ToListAsync(cancellationToken);

        reports.ForEach(r => r.ResolveReport());

        IDomainUser? author = await _userRepository.GetById(chapter.Volume.Series.CreatedBy ?? string.Empty);

        if (author is not null)
        {
            ReportService.RestrictUser(author, request.RestrictedDurationInDays);
            await _publisher.Publish(
                new AuthorRestrictedEvent(
                    author.Id,
                    author.RestrictPublishUntil ?? throw new Exception("RestrictPublishUntil cannot be null")),
                cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
