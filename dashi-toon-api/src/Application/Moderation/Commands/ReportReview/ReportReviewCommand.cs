using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Domain.Entities;

namespace DashiToon.Api.Application.Moderation.Commands.ReportReview;

[Authorize]
public sealed record ReportReviewCommand(Guid ReviewId, string Reason) : IRequest;

public sealed class ReportReviewCommandHandler : IRequestHandler<ReportReviewCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public ReportReviewCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task Handle(ReportReviewCommand request, CancellationToken cancellationToken)
    {
        Report? report = Report.CreateNewUserReport(
            _user.Id!,
            ReportType.Review,
            request.Reason,
            request.ReviewId
        );

        _context.Reports.Add(report);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
