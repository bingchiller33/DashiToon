using DashiToon.Api.Domain.Entities;

namespace DashiToon.Api.Application.Common.Interfaces;

public interface IUserRepository
{
    Task<string?> GetUsernameById(string id);
    Task<string?> GetUserAvatarById(string id);
    Task<IDomainUser?> GetUserById(string id);
    Task<IDomainUser?> GetById(string id);
    Task Update(IDomainUser user);
}
