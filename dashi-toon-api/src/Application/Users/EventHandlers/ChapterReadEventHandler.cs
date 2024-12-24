using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Events;

namespace DashiToon.Api.Application.Users.EventHandlers;

[Authorize]
public class ChapterReadEventHandler : INotificationHandler<ChapterReadEvent>
{
    private readonly IUser _user;
    private readonly IUserRepository _userRepository;
    private readonly IApplicationDbContext _dbContext;

    public ChapterReadEventHandler(IUser user, IUserRepository userRepository, IApplicationDbContext dbContext)
    {
        _user = user;
        _userRepository = userRepository;
        _dbContext = dbContext;
    }

    public async Task Handle(ChapterReadEvent notification, CancellationToken cancellationToken)
    {
        IDomainUser? user = await _userRepository.GetById(_user.Id ?? string.Empty);

        if (user is null)
        {
            throw new Exception();
        }

        Chapter? chapter = await _dbContext.Chapters
            .Include(c => c.Volume)
            .FirstAsync(c => c.Id == notification.ChapterId, cancellationToken);

        user.ReadChapter(notification.ChapterId);

        user.BookmarkChapter(notification.ChapterId, chapter.Volume.SeriesId);

        await _userRepository.Update(user);
    }
}
