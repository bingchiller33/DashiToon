using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Domain.Entities;

namespace DashiToon.Api.Application.Moderation.Commands.ReportComment;

[Authorize]
public sealed record ReportCommentCommand(Guid CommentId, string Reason) : IRequest;

public sealed class ReportCommentCommandHandler : IRequestHandler<ReportCommentCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public ReportCommentCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task Handle(ReportCommentCommand request, CancellationToken cancellationToken)
    {
        Report? report = Report.CreateNewUserReport(
            _user.Id!,
            ReportType.Comment,
            request.Reason,
            request.CommentId
        );

        _context.Reports.Add(report);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
