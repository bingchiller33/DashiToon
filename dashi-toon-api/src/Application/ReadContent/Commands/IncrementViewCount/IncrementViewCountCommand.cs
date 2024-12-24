using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Services;
using Microsoft.Extensions.Caching.Memory;

namespace DashiToon.Api.Application.ReadContent.Commands.IncrementViewCount;

public sealed record IncrementViewCountCommand(
    string? IpAddress,
    int ChapterId
) : IRequest;

public sealed class IncrementViewCountCommandHandler : IRequestHandler<IncrementViewCountCommand>
{
    private readonly IUser _user;
    private readonly ChapterAnalyticService _service;

    public IncrementViewCountCommandHandler(IUser user, ChapterAnalyticService service)
    {
        _user = user;
        _service = service;
    }

    public Task Handle(IncrementViewCountCommand request, CancellationToken cancellationToken)
    {
        _service.IncrementViewCount(request.ChapterId, _user.Id, request.IpAddress);

        return Task.CompletedTask;
    }
}
