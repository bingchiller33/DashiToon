using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Application.Images.Commands.UploadImage;
using DashiToon.Api.Domain.Constants;

namespace DashiToon.Api.Application.AuthorStudio.Chapters.Commands.UploadImages;

[Authorize]
[Restrict(Require = Restrictions.NotRestricted)]
public sealed record UploadChapterImageCommand(
    string ImageName,
    long ImageLength,
    string ContentType,
    Stream Data
) : IRequest<UploadImageResponse>;

public sealed class UploadChapterImageCommandHandler
    : IRequestHandler<UploadChapterImageCommand, UploadImageResponse?>
{
    private readonly ISender _sender;

    public UploadChapterImageCommandHandler(ISender sender)
    {
        _sender = sender;
    }

    public async Task<UploadImageResponse?> Handle(
        UploadChapterImageCommand request,
        CancellationToken cancellationToken)
    {
        return await _sender.Send(new UploadImageCommand(
                "chapters",
                request.ImageName,
                request.ImageLength,
                request.ContentType,
                request.Data),
            cancellationToken);
    }
}
