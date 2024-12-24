namespace DashiToon.Api.Application.Images.Commands.UploadImage;

public record UploadImageResponse(
    string FileName,
    long FileSize,
    int Width,
    int Height,
    string ImagePath);
