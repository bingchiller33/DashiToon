using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Domain.Constants;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Events;
using DashiToon.Api.Domain.Services;

namespace DashiToon.Api.Application.Moderation.Commands.ResolveSeriesReport;

[Authorize(Roles = Roles.Moderator)]
public sealed record ResolveSeriesReportCommand(int SeriesId, int RestrictedDurationInDays) : IRequest;

public sealed class ResolveSeriesReportCommandHandler : IRequestHandler<ResolveSeriesReportCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserRepository _userRepository;
    private readonly IPublisher _publisher;

    public ResolveSeriesReportCommandHandler(
        IApplicationDbContext context,
        IUserRepository userRepository,
        IPublisher publisher)
    {
        _context = context;
        _userRepository = userRepository;
        _publisher = publisher;
    }

    public async Task Handle(ResolveSeriesReportCommand request, CancellationToken cancellationToken)
    {
        Series? series = await _context.Series.FirstOrDefaultAsync(s => s.Id == request.SeriesId, cancellationToken);

        if (series is null)
        {
            throw new NotFoundException(request.SeriesId.ToString(), nameof(Series));
        }

        series.SuspendSeries();

        List<Report>? reports = await _context.Reports
            .Where(c => c.SeriesId == request.SeriesId)
            .ToListAsync(cancellationToken);

        reports.ForEach(r => r.ResolveReport());

        IDomainUser? author = await _userRepository.GetById(series.CreatedBy ?? string.Empty);

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
