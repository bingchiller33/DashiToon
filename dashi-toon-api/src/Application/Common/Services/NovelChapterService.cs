using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Domain.Entities;
using FluentValidation.Results;
using Ganss.Xss;
using HtmlAgilityPack;
using ValidationException = DashiToon.Api.Application.Common.Exceptions.ValidationException;

namespace DashiToon.Api.Application.Common.Services;

public class NovelChapterService
{
    private const long MaxTotalSize = 20 * 1024 * 1024;
    private readonly IHtmlSanitizer _htmlSanitizer;
    private readonly IImageStore _imageStore;

    public NovelChapterService(IImageStore imageStore)
    {
        _imageStore = imageStore;
        _htmlSanitizer = new HtmlSanitizer();
        _htmlSanitizer.AllowedAttributes.Add("data-img-name");
    }

    private static Task ValidateNovelContent(string content)
    {
        HtmlDocument htmlDocument = new();

        htmlDocument.LoadHtml(content);

        if (htmlDocument.ParseErrors.Any())
        {
            throw new ValidationException([
                new ValidationFailure(nameof(content), "The content must be in html format.")
            ]);
        }

        if (htmlDocument.DocumentNode.FirstChild is not { NodeType: HtmlNodeType.Element })
        {
            throw new ValidationException(new List<ValidationFailure>
            {
                new(nameof(content), "The content must contain valid HTML elements.")
            });
        }

        return Task.CompletedTask;
    }

    private async Task ValidateImageSize(string content)
    {
        HtmlDocument htmlDocument = new();

        htmlDocument.LoadHtml(content);

        long totalSize = 0L;

        HtmlNodeCollection? imageNodes = htmlDocument.DocumentNode.SelectNodes("//img");

        if (imageNodes is not null)
        {
            foreach (HtmlNode? imgNode in imageNodes)
            {
                if (imgNode is null)
                {
                    continue;
                }

                string? imageName = imgNode.GetAttributeValue("data-img-name", string.Empty);

                if (string.IsNullOrEmpty(imageName))
                {
                    continue;
                }

                (long Size, int Width, int Height) result = await _imageStore.GetMetadata("chapters/" + imageName);

                totalSize += result.Size;
            }
        }

        if (totalSize > MaxTotalSize)
        {
            throw new ValidationException([
                new ValidationFailure(nameof(content), "Total size exceed 20 MB")
            ]);
        }
    }

    public async Task<Chapter> CreateChapter(string title, string? thumbnail, string rawContent, string? note)
    {
        await ValidateNovelContent(rawContent);

        string sanitizedContent = _htmlSanitizer.Sanitize(rawContent);

        await ValidateImageSize(sanitizedContent);

        return Chapter.Create(title, thumbnail, sanitizedContent, note);
    }

    public async Task UpdateChapter(Chapter chapter, string title, string? thumbnail, string rawContent, string? note,
        bool isAutoSave = false)
    {
        await ValidateNovelContent(rawContent);

        string sanitizedContent = _htmlSanitizer.Sanitize(rawContent);

        await ValidateImageSize(sanitizedContent);

        if (isAutoSave)
        {
            chapter.Save(title, thumbnail, sanitizedContent, note);
        }
        else
        {
            chapter.Update(title, thumbnail, sanitizedContent, note);
        }
    }

    public async Task<string> ProcessContent(string content)
    {
        HtmlDocument htmlDoc = new();

        htmlDoc.LoadHtml(content);

        HtmlNodeCollection? imageNodes = htmlDoc.DocumentNode.SelectNodes("//img");

        if (imageNodes == null)
        {
            return htmlDoc.DocumentNode.InnerHtml;
        }

        foreach (HtmlNode? imgNode in imageNodes)
        {
            if (imgNode is null)
            {
                continue;
            }

            string? imageName = imgNode.GetAttributeValue("data-img-name", string.Empty);

            if (string.IsNullOrEmpty(imageName))
            {
                continue;
            }

            string url = await _imageStore.GetUrl("chapters/" + imageName);

            (long size, int width, int height) = await _imageStore.GetMetadata("chapters/" + imageName);

            imgNode.SetAttributeValue("src", url);
            imgNode.SetAttributeValue("data-img-width", width.ToString());
            imgNode.SetAttributeValue("data-img-height", height.ToString());
            imgNode.SetAttributeValue("data-img-size", size.ToString());
        }

        return htmlDoc.DocumentNode.InnerHtml;
    }

    public async Task<List<string>> GetImageUrls(string content)
    {
        HtmlDocument htmlDoc = new();

        htmlDoc.LoadHtml(content);

        HtmlNodeCollection? imageNodes = htmlDoc.DocumentNode.SelectNodes("//img");

        if (imageNodes == null)
        {
            return [];
        }

        List<string>? imageUrls = new(imageNodes.Count);

        foreach (HtmlNode? imgNode in imageNodes)
        {
            if (imgNode is null)
            {
                continue;
            }

            string? imageName = imgNode.GetAttributeValue("data-img-name", string.Empty);

            if (string.IsNullOrEmpty(imageName))
            {
                continue;
            }

            imageUrls.Add(await _imageStore.GetUrl("chapters/" + imageName));
        }

        return imageUrls;
    }

    public string GetTextContent(string content)
    {
        HtmlDocument htmlDoc = new();

        htmlDoc.LoadHtml(content);

        return htmlDoc.DocumentNode.InnerText;
    }
}
