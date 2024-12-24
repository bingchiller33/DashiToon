using DashiToon.Api.Application.Common.Interfaces;

namespace DashiToon.Api.Application.AuthorStudio.Chapters.Commands.UploadThumbnail;

public class UploadChapterThumbnailCommandValidator : AbstractValidator<UploadChapterThumbnailCommand>
{
    private const double RequiredAspectRatio = 0.75;
    private readonly IImageService _imageService;

    public UploadChapterThumbnailCommandValidator(IImageService imageService)
    {
        _imageService = imageService;

        RuleFor(x => x.Data)
            .Must(HaveAspectRatio)
            .WithMessage("Thumbnail image must have an aspect ratio of 3:4");
    }

    private bool HaveAspectRatio(Stream data)
    {
        MemoryStream copy = new();

        data.CopyTo(copy);

        data.Seek(0, SeekOrigin.Begin);
        copy.Seek(0, SeekOrigin.Begin);

        (int width, int height) = _imageService.GetDimensions(copy);

        return Math.Abs(((double)width / height) - RequiredAspectRatio) < 0.01;
    }
}
