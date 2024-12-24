using DashiToon.Api.Application.AuthorStudio.Chapters.Models;
using DashiToon.Api.Application.Common.Interfaces;

namespace DashiToon.Api.Application.Images.Queries.GetImageUrl;

public sealed record GetImageUrlQuery(string ImageName, string Type) : IRequest<ImageInfo>;

public sealed class GetImageUrlQueryHandler : IRequestHandler<GetImageUrlQuery, ImageInfo>
{
    private readonly IImageStore _imageStore;

    public GetImageUrlQueryHandler(IImageStore imageStore)
    {
        _imageStore = imageStore;
    }

    public async Task<ImageInfo> Handle(GetImageUrlQuery request, CancellationToken cancellationToken)
    {
        string url = await _imageStore.GetUrl($"{request.Type}/{request.ImageName}");

        (long fileSize, int width, int height) = await _imageStore.GetMetadata($"{request.Type}/{request.ImageName}");

        return new ImageInfo(
            url,
            request.ImageName,
            fileSize,
            width,
            height
        );
    }
}
