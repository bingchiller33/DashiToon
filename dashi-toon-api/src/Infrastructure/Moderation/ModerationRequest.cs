namespace DashiToon.Api.Infrastructure.Moderation;

public class ModerationRequest
{
    public string Model { get; set; } = null!;
    public List<object> Input { get; init; } = [];
}

public class ImageContent(string imageUrl)
{
    public virtual string Type => "image_url";
    public object ImageUrl { get; init; } = new { Url = imageUrl };
}

public class TextContent(string text)
{
    public virtual string Type => "text";
    public string Text { get; init; } = text;
}
