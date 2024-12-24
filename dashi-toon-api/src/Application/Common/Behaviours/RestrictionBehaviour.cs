using System.Reflection;
using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Domain.Constants;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Services;

namespace DashiToon.Api.Application.Common.Behaviours;

public class RestrictionBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IUser _user;
    private readonly IUserRepository _userRepository;

    public RestrictionBehaviour(IUser user, IUserRepository userRepository)
    {
        _user = user;
        _userRepository = userRepository;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        RestrictAttribute? restrictAttribute = request.GetType().GetCustomAttribute<RestrictAttribute>();

        if (restrictAttribute is null)
        {
            return await next();
        }

        IDomainUser? user = await _userRepository.GetUserById(_user.Id!);

        if (user is null)
        {
            throw new UnauthorizedAccessException();
        }

        switch (restrictAttribute.Require)
        {
            case Restrictions.NotMuted when !ReportService.IsUserAllowedToCommentOrReview(user):
            case Restrictions.NotRestricted when !ReportService.IsUserAllowedToPublish(user):
                throw new GoneException();
        }

        return await next();
    }
}
