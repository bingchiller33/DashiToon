using System.Text.Json;
using DashiToon.Api.Application.AuthorStudio.Chapters.Models;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Domain.Entities;
using FluentValidation.Results;
using ValidationException = DashiToon.Api.Application.Common.Exceptions.ValidationException;

namespace DashiToon.Api.Application.Common.Services;

public class ComicChapterService
{
    private const long MaxTotalSize = 20 * 1024 * 1024;
    private readonly IImageStore _imageStore;

    public ComicChapterService(IImageStore imageStore)
    {
        _imageStore = imageStore;
    }

    private async Task ValidateImageSize(List<string> content)
    {
        long totalSize = 0L;

        foreach (string image in content)
        {
            (long Size, int Width, int Height) result = await _imageStore.GetMetadata($"chapters/{image}");

            totalSize += result.Size;
        }

        if (totalSize > MaxTotalSize)
        {
            throw new ValidationException([
                new ValidationFailure(nameof(content), "Total size exceed 20 MB")
            ]);
        }
    }

    public async Task<Chapter> CreateChapter(string title, string? thumbnail, List<string> content, string? note)
    {
        await ValidateImageSize(content);

        return Chapter.Create(
            title,
            thumbnail,
            JsonSerializer.Serialize(content),
            note);
    }

    public async Task UpdateChapter(Chapter chapter,
        string title,
        string? thumbnail,
        List<string> content,
        string? note,
        bool isAutoSave = false)
    {
        await ValidateImageSize(content);

        if (isAutoSave)
        {
            chapter.Save(
                title,
                thumbnail,
                JsonSerializer.Serialize(content),
                note);
        }
        else
        {
            chapter.Update(
                title,
                thumbnail,
                JsonSerializer.Serialize(content),
                note);
        }
    }


    public async Task<List<ImageInfo>> ProcessContent(string content)
    {
        List<string> images = JsonSerializer.Deserialize<List<string>>(content) ?? [];

        List<ImageInfo> imageInfos = new(images.Count);

        foreach (string imageName in images)
        {
            string url = await _imageStore.GetUrl("chapters/" + imageName);

            (long Size, int Width, int Height) metaData = await _imageStore.GetMetadata("chapters/" + imageName);

            imageInfos.Add(new ImageInfo(url, imageName, metaData.Size, metaData.Width, metaData.Height));
        }

        return imageInfos;
    }

    public async Task<List<string>> GetImageUrls(string content)
    {
        List<string> images = JsonSerializer.Deserialize<List<string>>(content) ?? [];

        List<string>? imageUrls = new();

        foreach (string imageName in images)
        {
            string url = await _imageStore.GetUrl("chapters/" + imageName);

            imageUrls.Add(url);
        }

        return imageUrls;
    }
}
