namespace DashiToon.Api.Application.Common.Interfaces;

public interface IImageStore
{
    Task<int> Upload(
        string objectKey,
        string contentType,
        long size,
        int width,
        int height,
        Stream data);

    Task<string> GetUrl(string key, DateTime? expiryDate = null);
    Task<(long Size, int Width, int Height)> GetMetadata(string key);
    Task RemoveImage(string key);
}
