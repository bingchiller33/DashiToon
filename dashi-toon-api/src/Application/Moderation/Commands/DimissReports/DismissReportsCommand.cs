using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Domain.Constants;
using DashiToon.Api.Domain.Entities;

namespace DashiToon.Api.Application.Moderation.Commands.DimissReports;

[Authorize(Roles = Roles.Moderator)]
public sealed record DismissReportsCommand(string EntityId, ReportType Type) : IRequest;

public sealed class DismissReportsCommandHandler : IRequestHandler<DismissReportsCommand>
{
    private readonly IApplicationDbContext _context;

    public DismissReportsCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DismissReportsCommand request, CancellationToken cancellationToken)
    {
        List<Report>? reports = request.Type switch
        {
            ReportType.Review => await _context.Reports
                .Where(r => r.ReviewId == Guid.Parse(request.EntityId) && r.ReportStatus == ReportStatus.Pending)
                .ToListAsync(cancellationToken),
            ReportType.Comment => await _context.Reports
                .Where(r => r.CommentId == Guid.Parse(request.EntityId) && r.ReportStatus == ReportStatus.Pending)
                .ToListAsync(cancellationToken),
            ReportType.Content => await _context.Reports
                .Where(r => r.ChapterId == int.Parse(request.EntityId) && r.ReportStatus == ReportStatus.Pending)
                .ToListAsync(cancellationToken),
            ReportType.Series => await _context.Reports
                .Where(r => r.SeriesId == int.Parse(request.EntityId) && r.ReportStatus == ReportStatus.Pending)
                .ToListAsync(cancellationToken),
            _ => throw new ArgumentOutOfRangeException()
        };

        reports.ForEach(r => r.DismissReport());

        _context.Reports.UpdateRange(reports);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
