using DashiToon.Api.Application.Common.Interfaces;

namespace DashiToon.Api.Application.Images.Commands.UploadImage;

public sealed record UploadImageCommand(
    string Folder,
    string ImageName,
    long ImageSize,
    string ContentType,
    Stream Data
) : IRequest<UploadImageResponse?>;

public sealed class UploadImageCommandHandler : IRequestHandler<UploadImageCommand, UploadImageResponse?>
{
    private readonly IImageService _imageService;
    private readonly IImageStore _imageStore;

    public UploadImageCommandHandler(IImageStore imageStore, IImageService imageService)
    {
        _imageStore = imageStore;
        _imageService = imageService;
    }

    public async Task<UploadImageResponse?> Handle(UploadImageCommand request, CancellationToken cancellationToken)
    {
        (int width, int height) = _imageService.GetDimensions(request.Data);

        string fileName = $"{Guid.NewGuid()}{Path.GetExtension(request.ImageName)}";

        string imagePath = request.Folder + "/" + fileName;

        int result = await _imageStore.Upload(
            imagePath,
            request.ContentType,
            request.ImageSize,
            width,
            height,
            request.Data);

        return result == 1
            ? new UploadImageResponse(
                fileName,
                request.ImageSize,
                width,
                height,
                imagePath)
            : null;
    }
}
