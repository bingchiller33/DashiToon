namespace DashiToon.Api.Application.Administrator.Users.Models;

public sealed record UserVm(
    string UserId,
    string? UserName,
    string? Email,
    string? PhoneNumber,
    IList<string> Roles
);
