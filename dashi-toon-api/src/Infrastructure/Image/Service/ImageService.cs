using DashiToon.Api.Application.Common.Interfaces;

namespace DashiToon.Api.Infrastructure.Image.Service;

public class ImageService : IImageService
{
    public (int width, int height) GetDimensions(Stream imageStream)
    {
        using SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(imageStream);

        return (image.Width, image.Height);
    }
}
