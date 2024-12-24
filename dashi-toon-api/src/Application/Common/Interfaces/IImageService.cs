namespace DashiToon.Api.Application.Common.Interfaces;

public interface IImageService
{
    public (int width, int height) GetDimensions(Stream imageStream);
}
