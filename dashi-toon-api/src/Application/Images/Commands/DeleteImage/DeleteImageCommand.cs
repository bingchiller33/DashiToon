using DashiToon.Api.Application.Common.Interfaces;

namespace DashiToon.Api.Application.Images.Commands.DeleteImage;

public sealed record DeleteImageCommand(string ImageName, string Type) : IRequest;

public sealed class DeleteImageCommandHandler : IRequestHandler<DeleteImageCommand>
{
    private readonly IImageStore _imageStore;

    public DeleteImageCommandHandler(IImageStore imageStore)
    {
        _imageStore = imageStore;
    }

    public async Task Handle(DeleteImageCommand request, CancellationToken cancellationToken)
    {
        await _imageStore.RemoveImage($"{request.Type}/{request.ImageName}");
    }
}
