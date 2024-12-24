using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using DashiToon.Api.Application.Subscriptions.Commands.HandleSubscriptionEvent;
using DashiToon.Api.Application.Subscriptions.Commands.Models;
using DashiToon.Api.Application.Subscriptions.Commands.ProcessKanaGoldPackPurchase;
using DashiToon.Api.Application.Subscriptions.Commands.PurchaseKanaGoldPack;
using DashiToon.Api.Application.Subscriptions.Queries.GetKanaGoldPacks;
using Force.Crc32;

namespace DashiToon.Api.Web.Endpoints;

public class Subscriptions : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .RequireAuthorization()
            .MapGet(GetKanaGoldPacks, "kana-packs")
            .MapPost(PurchaseKanaGoldPack, "kana-packs/{id}/purchase");

        app.MapGroup(this)
            .MapPost(ProcessKanaGoldPack, "kana-packs/{orderId}/capture")
            .MapPost(HandleWebhook, "/webhook");
    }

    public async Task<List<KanaGoldPackVm>> GetKanaGoldPacks(ISender sender)
    {
        return await sender.Send(new GetKanaGoldPacks());
    }

    public async Task<IResult> PurchaseKanaGoldPack(
        ISender sender,
        string id,
        PurchaseKanaGoldPackCommand command)
    {
        OrderResult result = await sender.Send(command);

        return Results.Json(data: result.Data, statusCode: result.StatusCode);
    }

    public async Task<IResult> ProcessKanaGoldPack(ISender sender, string orderId)
    {
        OrderResult result = await sender.Send(new ProcessKanaGoldPackPurchaseCommand(orderId));

        return Results.Json(data: result.Data, statusCode: result.StatusCode);
    }

    public async Task<IResult> HandleWebhook(ISender sender, HttpRequest request, IConfiguration configuration)
    {
        string json = await new StreamReader(request.Body).ReadToEndAsync();
        IHeaderDictionary headers = request.Headers;

        if (!await VerifyEvent(json, headers, configuration["Paypal:WebhookId"]!))
        {
            return Results.BadRequest();
        }

        await sender.Send(new HandleEventCommand(json));

        return Results.Ok("Success");
    }

    public async Task<bool> VerifyEvent(string json, IHeaderDictionary headers, string webHookId)
    {
        string transmissionId = headers["paypal-transmission-id"].ToString();
        string timeStamp = headers["paypal-transmission-time"].ToString();
        string signature = headers["paypal-transmission-sig"].ToString();
        string certUrl = headers["paypal-cert-url"].ToString();

        uint crc32Checksum = CalculateCrc32Checksum(json);

        // Create the message that was originally signed
        string signedMessage = $"{transmissionId}|{timeStamp}|{webHookId}|{crc32Checksum}";

        // Download PayPal's certificate
        string certPem = await DownloadCertificate(certUrl);

        // Verify the signature
        return VerifySignature(signature, signedMessage, certPem);
    }

    private async Task<string> DownloadCertificate(string certUrl)
    {
        CertificateDownloader downloader = new();
        return await downloader.DownloadCertificate(certUrl);
    }

    private bool VerifySignature(string signatureBase64, string message, string certPem)
    {
        // Convert the base64-encoded signature into bytes
        byte[] signatureBytes = Convert.FromBase64String(signatureBase64);

        // Load the PayPal certificate
        X509Certificate2 cert = new(Encoding.ASCII.GetBytes(certPem));

        // Get the RSA public key from the certificate
        using RSA? rsa = cert.GetRSAPublicKey();
        // Create a verification object
        RSAPKCS1SignatureDeformatter verifier = new(rsa!);
        verifier.SetHashAlgorithm("SHA256");

        // Compute the hash of the signed message
        byte[] hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(message));

        // Verify the signature
        return verifier.VerifySignature(hashBytes, signatureBytes);
    }

    private static uint CalculateCrc32Checksum(string input)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(input);
        return Crc32Algorithm.Compute(bytes);
    }
}

public class CertificateDownloader
{
    private const string CacheDir = "CertificateCache"; // Define your cache directory

    public async Task<string> DownloadCertificate(string certUrl)
    {
        // Create the cache directory if it doesn't exist
        if (!Directory.Exists(CacheDir))
        {
            Directory.CreateDirectory(CacheDir);
        }

        // Generate a cache key by sanitizing the URL to create a valid file name
        string cacheKey = GenerateCacheKey(certUrl);
        string cacheFilePath = Path.Combine(CacheDir, cacheKey);

        // Check if the certificate is cached
        if (File.Exists(cacheFilePath))
        {
            return await File.ReadAllTextAsync(cacheFilePath);
        }

        // If not cached, download the certificate from the URL
        using (HttpClient httpClient = new())
        {
            string certPem = await httpClient.GetStringAsync(certUrl);

            // Cache the certificate to the file system
            await File.WriteAllTextAsync(cacheFilePath, certPem);

            return certPem;
        }
    }

    private string GenerateCacheKey(string url)
    {
        // Replace invalid file name characters with hyphens
        string sanitizedKey = string.Concat(url.Select(c => char.IsLetterOrDigit(c) ? c : '-'));
        return sanitizedKey;
    }
}
