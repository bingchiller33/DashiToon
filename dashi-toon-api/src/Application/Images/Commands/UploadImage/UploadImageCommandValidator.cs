namespace DashiToon.Api.Application.Images.Commands.UploadImage;

public class UploadImageCommandValidator : AbstractValidator<UploadImageCommand>
{
    private const long MaxSize = 2 * 1024 * 1024;
    private static readonly string[] permittedExtensions = { ".jpg", ".jpeg", ".png" };

    public UploadImageCommandValidator()
    {
        RuleFor(x => x.ImageName)
            .Must(AcceptFormat)
            .WithMessage("Only JPG, JPEG and PNG formats are supported");

        RuleFor(x => x.ImageSize)
            .Must(FulfillLength);
    }

    private static bool FulfillLength(long length)
    {
        return length <= MaxSize;
    }

    private static bool AcceptFormat(string contentType)
    {
        string ext = Path.GetExtension(contentType).ToLowerInvariant();

        return !string.IsNullOrEmpty(ext) && permittedExtensions.Contains(ext);
    }
}
