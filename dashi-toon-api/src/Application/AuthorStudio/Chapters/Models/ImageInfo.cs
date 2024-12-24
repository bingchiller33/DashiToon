namespace DashiToon.Api.Application.AuthorStudio.Chapters.Models;

public sealed record ImageInfo(
    string ImageUrl,
    string ImageName,
    long ImageSize,
    int ImageWidth,
    int ImageHeight
);
