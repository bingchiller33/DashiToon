using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Domain.Constants;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Events;
using DashiToon.Api.Domain.Services;

namespace DashiToon.Api.Application.Moderation.Commands.ResolveReviewReport;

[Authorize(Roles = Roles.Moderator)]
public sealed record ResolveReviewReportCommand(Guid ReviewId, int MutedDurationInDays) : IRequest;

public sealed class ResolveReviewReportCommandHandler : IRequestHandler<ResolveReviewReportCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserRepository _userRepository;
    private readonly IPublisher _publisher;

    public ResolveReviewReportCommandHandler(IApplicationDbContext context, IUserRepository userRepository,
        IPublisher publisher)
    {
        _context = context;
        _userRepository = userRepository;
        _publisher = publisher;
    }

    public async Task Handle(ResolveReviewReportCommand request, CancellationToken cancellationToken)
    {
        Review? review = await _context.Reviews.FirstOrDefaultAsync(r => r.Id == request.ReviewId, cancellationToken);

        if (review is null)
        {
            throw new NotFoundException(request.ReviewId.ToString(), nameof(Review));
        }

        _context.Reviews.Remove(review);

        List<Report>? reports = await _context.Reports
            .Where(c => c.ReviewId == request.ReviewId)
            .ToListAsync(cancellationToken);

        reports.ForEach(r => r.ResolveReport());

        IDomainUser? reviewUser = await _userRepository.GetById(review.UserId);

        if (reviewUser is not null)
        {
            ReportService.MuteUser(reviewUser, request.MutedDurationInDays);
            await _publisher.Publish(
                new UserMutedEvent(
                    reviewUser.Id,
                    reviewUser.MuteUntil ?? throw new Exception("MuteUntil cannot be null")),
                cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
