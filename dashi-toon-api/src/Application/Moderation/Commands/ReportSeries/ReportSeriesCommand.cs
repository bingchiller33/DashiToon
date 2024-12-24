using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Domain.Entities;

namespace DashiToon.Api.Application.Moderation.Commands.ReportSeries;

[Authorize]
public sealed record ReportSeriesCommand(int SeriesId, string Reason) : IRequest;

public sealed class ReportSeriesCommandHandler : IRequestHandler<ReportSeriesCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public ReportSeriesCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task Handle(ReportSeriesCommand request, CancellationToken cancellationToken)
    {
        Report? report = Report.CreateNewUserReport(
            _user.Id!,
            ReportType.Series,
            request.Reason,
            request.SeriesId
        );

        _context.Reports.Add(report);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
