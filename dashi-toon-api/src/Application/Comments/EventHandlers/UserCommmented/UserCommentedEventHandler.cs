using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Events;

namespace DashiToon.Api.Application.Comments.EventHandlers.UserCommmented;

public sealed class UserCommentedEventHandler : INotificationHandler<UserCommentedEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly IModerationService _moderationService;

    public UserCommentedEventHandler(IApplicationDbContext context, IModerationService moderationService)
    {
        _context = context;
        _moderationService = moderationService;
    }

    public async Task Handle(UserCommentedEvent notification, CancellationToken cancellationToken)
    {
        ModerationAnalysis? analysis = await _moderationService.ModerateComment(notification.Comment.Content);

        if (!analysis.Flagged)
        {
            return;
        }

        Report? report = Report.CreateNewSystemReport(ReportType.Comment, notification.Comment.Id);

        report.AddAnalytics(analysis);

        _context.Reports.Add(report);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
