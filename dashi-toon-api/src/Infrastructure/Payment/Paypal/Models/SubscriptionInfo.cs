using System.Text.Json.Serialization;

namespace DashiToon.Api.Infrastructure.Payment.Paypal.Models;

public class SubscriptionInfo
{
    [JsonPropertyName("status")] public string Status { get; set; } = string.Empty;

    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;

    [JsonPropertyName("create_time")] public DateTimeOffset CreateTime { get; set; }
    [JsonPropertyName("links")] public List<PayPalLink> Links { get; set; } = new();

    // Helper methods to easily get specific links
    public PayPalLink? GetApprovalLink()
    {
        return Links.FirstOrDefault(l => l.Rel == "approve");
    }

    public PayPalLink? GetEditLink()
    {
        return Links.FirstOrDefault(l => l.Rel == "edit");
    }

    public PayPalLink? GetSelfLink()
    {
        return Links.FirstOrDefault(l => l.Rel == "self");
    }
}

public class PayPalLink
{
    [JsonPropertyName("href")] public string Href { get; set; } = string.Empty;
    [JsonPropertyName("rel")] public string Rel { get; set; } = string.Empty;

    [JsonPropertyName("method")] public string Method { get; set; } = string.Empty;
}
