using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using DashiToon.Api.Application.Common.Interfaces;

namespace DashiToon.Api.Infrastructure.Image.Store;

public class ImageStore : IImageStore
{
    private readonly string _bucketName;
    private readonly IAmazonS3 _s3;

    public ImageStore(IAmazonS3 s3)
    {
        _s3 = s3;
        _bucketName = "dashitoon";
    }

    public async Task<int> Upload(
        string objectKey,
        string contentType,
        long size,
        int width,
        int height,
        Stream data)
    {
        PutObjectRequest putObjectRequest = new()
        {
            BucketName = _bucketName,
            Key = $"{objectKey}",
            ContentType = contentType,
            Metadata =
            {
                ["x-amz-meta-file-size"] = size.ToString(),
                ["x-amz-meta-image-width"] = width.ToString(),
                ["x-amz-meta-image-height"] = height.ToString()
            },
            InputStream = data
        };

        PutObjectResponse? result = await _s3.PutObjectAsync(putObjectRequest);

        return result.HttpStatusCode == HttpStatusCode.OK ? 1 : 0;
    }

    public async Task<string> GetUrl(string key, DateTime? expiryDate = null)
    {
        GetPreSignedUrlRequest getPreSignedUrlRequest = new()
        {
            BucketName = _bucketName, Key = key, Expires = expiryDate ?? DateTime.UtcNow.AddMinutes(5)
        };

        return await _s3.GetPreSignedURLAsync(getPreSignedUrlRequest);
    }

    public async Task<(long Size, int Width, int Height)> GetMetadata(string key)
    {
        GetObjectMetadataRequest getMetadataRequest = new() { BucketName = _bucketName, Key = key };

        GetObjectMetadataResponse? response = await _s3.GetObjectMetadataAsync(getMetadataRequest);

        return new ValueTuple<long, int, int>(
            long.Parse(response.Metadata["x-amz-meta-file-size"]),
            int.Parse(response.Metadata["x-amz-meta-image-width"]),
            int.Parse(response.Metadata["x-amz-meta-image-height"]));
    }

    public async Task RemoveImage(string key)
    {
        DeleteObjectRequest deleteObjectRequest = new() { BucketName = _bucketName, Key = key };

        await _s3.DeleteObjectAsync(deleteObjectRequest);
    }
}
