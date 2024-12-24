using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Domain.Entities;

namespace DashiToon.Api.Application.Moderation.Commands.ReportChapter;

[Authorize]
public sealed record ReportChapterCommand(int ChapterId, string Reason) : IRequest;

public sealed class ReportChapterCommandHandler : IRequestHandler<ReportChapterCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public ReportChapterCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task Handle(ReportChapterCommand request, CancellationToken cancellationToken)
    {
        Report? report = Report.CreateNewUserReport(
            _user.Id!,
            ReportType.Content,
            request.Reason,
            request.ChapterId
        );

        _context.Reports.Add(report);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
