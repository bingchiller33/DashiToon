using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Domain.Constants;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Events;
using DashiToon.Api.Domain.Services;

namespace DashiToon.Api.Application.Moderation.Commands.ResolveCommentReport;

[Authorize(Roles = Roles.Moderator)]
public sealed record ResolveCommentReportCommand(Guid CommentId, int MutedDurationInDays) : IRequest;

public sealed class ResolveCommentReportCommandHandler : IRequestHandler<ResolveCommentReportCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserRepository _userRepository;
    private readonly IPublisher _publisher;

    public ResolveCommentReportCommandHandler(
        IApplicationDbContext context,
        IUserRepository userRepository,
        IPublisher publisher)
    {
        _context = context;
        _userRepository = userRepository;
        _publisher = publisher;
    }

    public async Task Handle(ResolveCommentReportCommand request, CancellationToken cancellationToken)
    {
        Comment? comment =
            await _context.Comments.FirstOrDefaultAsync(c => c.Id == request.CommentId, cancellationToken);

        if (comment is null)
        {
            throw new NotFoundException(request.CommentId.ToString(), nameof(Comment));
        }

        _context.Comments.Remove(comment);

        List<Report>? reports = await _context.Reports
            .Where(c => c.CommentId == request.CommentId)
            .ToListAsync(cancellationToken);

        reports.ForEach(r => r.ResolveReport());

        IDomainUser? commentUser = await _userRepository.GetById(comment.UserId);

        if (commentUser is not null)
        {
            ReportService.MuteUser(commentUser, request.MutedDurationInDays);
            await _publisher.Publish(
                new UserMutedEvent(
                    commentUser.Id,
                    commentUser.MuteUntil ?? throw new Exception("MuteUntil cannot be null")),
                cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
