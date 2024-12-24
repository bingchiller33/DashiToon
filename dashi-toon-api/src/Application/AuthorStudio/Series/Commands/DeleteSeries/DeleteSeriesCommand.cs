using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Domain.Constants;

namespace DashiToon.Api.Application.AuthorStudio.Series.Commands.DeleteSeries;

[Authorize]
[Restrict(Require = Restrictions.NotRestricted)]
public record DeleteSeriesCommand(int Id) : IRequest;

public class DeleteSeriesCommandHandler : IRequestHandler<DeleteSeriesCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUser _user;

    public DeleteSeriesCommandHandler(IApplicationDbContext dbContext, IUser user)
    {
        _dbContext = dbContext;
        _user = user;
    }

    public async Task Handle(DeleteSeriesCommand request, CancellationToken cancellationToken)
    {
        Domain.Entities.Series? series = await _dbContext.Series
            .Include(x => x.Genres)
            .Include(x => x.Volumes)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (series is null)
        {
            throw new NotFoundException(request.Id.ToString(), nameof(Series));
        }

        if (series.CreatedBy != _user.Id)
        {
            throw new ForbiddenAccessException();
        }

        _dbContext.Series.Remove(series);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
