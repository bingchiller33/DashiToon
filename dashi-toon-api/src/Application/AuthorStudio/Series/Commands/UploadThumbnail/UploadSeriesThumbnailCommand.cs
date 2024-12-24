using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Application.Images.Commands.UploadImage;
using DashiToon.Api.Domain.Constants;

namespace DashiToon.Api.Application.AuthorStudio.Series.Commands.UploadThumbnail;

[Authorize]
[Restrict(Require = Restrictions.NotRestricted)]
public sealed record UploadSeriesThumbnailCommand(
    string ImageName,
    long ImageLength,
    string ContentType,
    Stream Data
) : IRequest<UploadImageResponse?>;

public sealed class UploadThumbnailCommandHandler : IRequestHandler<UploadSeriesThumbnailCommand, UploadImageResponse?>
{
    private readonly ISender _sender;

    public UploadThumbnailCommandHandler(ISender sender)
    {
        _sender = sender;
    }

    public async Task<UploadImageResponse?> Handle(
        UploadSeriesThumbnailCommand request,
        CancellationToken cancellationToken)
    {
        return await _sender.Send(new UploadImageCommand(
                "thumbnails",
                request.ImageName,
                request.ImageLength,
                request.ContentType,
                request.Data),
            cancellationToken);
    }
}
